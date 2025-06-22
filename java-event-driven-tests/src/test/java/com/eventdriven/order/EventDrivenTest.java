package com.eventdriven.order;

import com.eventdriven.order.model.Order;
import com.eventdriven.order.util.TestKafkaProducer;
import com.fasterxml.jackson.core.JsonProcessingException;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.awaitility.Awaitility;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.testcontainers.containers.GenericContainer;
import org.testcontainers.kafka.KafkaContainer;
import org.testcontainers.containers.Network;
import org.testcontainers.containers.wait.strategy.Wait;
import org.testcontainers.utility.DockerImageName;
import org.testcontainers.images.builder.ImageFromDockerfile;
import org.testng.annotations.AfterClass;
import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;
import redis.clients.jedis.Jedis;

import java.nio.file.Paths;
import java.time.Duration;
import java.util.Collections;
import java.util.Optional;
import java.util.Properties;
import java.util.UUID;

import static com.eventdriven.order.redis.OrderStatusRepository.REDIS_KEY_PREFIX;
import static org.testng.Assert.assertEquals;
import static org.testng.Assert.assertTrue;
import static org.testng.Assert.fail;

public class EventDrivenTest {

    private static final Logger LOGGER = LoggerFactory.getLogger(EventDrivenTest.class);

    private static final String TOPIC = "order_created_topic";
    private static final String DLQ_TOPIC = "order_dead_letter_topic";
    private static final String REDIS_IMAGE = "redis:7.0.2";
    private static final String KAFKA_ALIAS = "kafka";
    private static final String REDIS_ALIAS = "redis";
    private static final int REDIS_PORT = 6379;
    private static final int ORDER_SERVICE_PORT = 8080;
    private static final String ENV_KAFKA_BOOTSTRAP = "SPRING_KAFKA_BOOTSTRAP_SERVERS";
    private static final String ENV_REDIS_HOST = "SPRING_DATA_REDIS_HOST";
    private static final String ENV_MANAGEMENT_ENDPOINTS = "MANAGEMENT_ENDPOINTS_WEB_EXPOSURE_INCLUDE";
    private static final String MANAGEMENT_ENDPOINTS_VALUE = "health";
    private static final String HEALTH_ENDPOINT = "/actuator/health";
    private static final Duration HEALTH_TIMEOUT = Duration.ofSeconds(120);
   
    private static Network network;

    private static KafkaContainer kafka;
    private static GenericContainer<?> redis;
    private static GenericContainer<?> orderServiceApp;

    private static Jedis jedis;
    private static TestKafkaProducer testKafkaProducer;

    @BeforeClass(alwaysRun = true)
    public static void setupTestClass() {
        try {
            network = Network.newNetwork();
        } catch (Exception e) {
            LOGGER.error("Failed to create Docker network: {}", e.getMessage(), e);
            throw new RuntimeException("Failed to create Docker network", e);
        }
        startContainers();
        try {
            jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
            jedis.ping(); // Test connection
        } catch (Exception e) {
            LOGGER.error("Failed to connect to Redis: {}", e.getMessage(), e);
            throw new RuntimeException("Failed to connect to Redis", e);
        }
        testKafkaProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);

        // Add shutdown hook for cleanup
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            try {
                cleanupTestClass();
            } catch (Exception ex) {
                LOGGER.warn("Exception during shutdown hook cleanup", ex);
            }
        }));
    }

    private static void startContainers() {
        kafka = new KafkaContainer(DockerImageName.parse("confluentinc/cp-kafka:7.5.0"))
                .withNetwork(network)
                .withNetworkAliases(KAFKA_ALIAS);
        redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                .withExposedPorts(REDIS_PORT)
                .withNetwork(network)
                .withNetworkAliases(REDIS_ALIAS);

        orderServiceApp = new GenericContainer<>(
                new ImageFromDockerfile().withDockerfile(Paths.get("Dockerfile")))
                .withExposedPorts(ORDER_SERVICE_PORT)
                .withNetwork(network)
                .dependsOn(kafka, redis)
                .withEnv(ENV_KAFKA_BOOTSTRAP, KAFKA_ALIAS + ":9092")
                .withEnv(ENV_REDIS_HOST, REDIS_ALIAS)
                .withEnv(ENV_MANAGEMENT_ENDPOINTS, MANAGEMENT_ENDPOINTS_VALUE)
                .waitingFor(Wait.forHttp(HEALTH_ENDPOINT).forStatusCode(200)
                        .withStartupTimeout(HEALTH_TIMEOUT));

        LOGGER.info("Starting Testcontainers... This will take a moment.");
        try {
            kafka.start();
            redis.start();
            orderServiceApp.start();
            LOGGER.info("Testcontainers started successfully.");
        } catch (Exception e) {
            LOGGER.error("Failed to start containers: {}", e.getMessage(), e);
            if (orderServiceApp != null)
                orderServiceApp.stop();
            if (redis != null)
                redis.stop();
            if (kafka != null)
                kafka.stop();
            throw new RuntimeException("Failed to start containers", e);
        }
    }

    @AfterClass(alwaysRun = true)
    public static void cleanupTestClass() {
        LOGGER.info("Stopping Testcontainers...");
        try {
            if (jedis != null)
                jedis.close();
        } catch (Exception e) {
            LOGGER.warn("Failed to close Jedis", e);
        }
        try {
            if (testKafkaProducer != null)
                testKafkaProducer.close();
        } catch (Exception e) {
            LOGGER.warn("Failed to close TestKafkaProducer", e);
        }
        if (orderServiceApp != null) {
            try {
                orderServiceApp.stop();
                orderServiceApp.close();
            } catch (Exception e) {
                LOGGER.warn("Failed to close orderServiceApp", e);
            }
        }
        if (redis != null) {
            try {
                redis.stop();
                redis.close();
            } catch (Exception e) {
                LOGGER.warn("Failed to close redis", e);
            }
        }
        if (kafka != null) {
            try {
                kafka.stop();
            } catch (Exception e) {
                LOGGER.warn("Failed to stop kafka", e);
            }
            try {
                kafka.close();
            } catch (Exception e) {
                LOGGER.warn("Failed to close kafka", e);
            }
        }
        if (network != null) {
            try {
                network.close();
            } catch (Exception e) {
                LOGGER.warn("Failed to close Docker network", e);
            }
            network = null;
        }
        LOGGER.info("Testcontainers stopped.");
    }

    @BeforeMethod
    public void setupTestMethod() {
        jedis.flushDB();
    }

    @Test(groups = "integration", description = "Should process an order event and update status in Redis to PROCESSED")
    public void testOrderEventEndToEnd() {
        String orderId = UUID.randomUUID().toString();
        Order testOrder = Order.builder()
                .id(orderId)
                .description("A test order from the E2E suite")
                .build();

        LOGGER.info("Sending order {} to Kafka topic {}", orderId, TOPIC);

        try {
            testKafkaProducer.sendOrder(testOrder);
        } catch (JsonProcessingException e) {
            fail("Failed to serialize order: " + e.getMessage(), e);
        }

        LOGGER.info("Awaiting for Redis key {} to be PROCESSED...", REDIS_KEY_PREFIX + orderId);
        Awaitility.await()
                .atMost(Duration.ofSeconds(15))
                .pollInterval(Duration.ofMillis(500))
                .untilAsserted(() -> {
                    String status = jedis.get(REDIS_KEY_PREFIX + orderId);
                    LOGGER.info("Polling Redis for order {}. Current status: {}", orderId, status);
                    assertEquals(status, "PROCESSED",
                            "Order status in Redis should be 'PROCESSED' for order " + orderId);
                });

        LOGGER.info("Successfully verified order {} is PROCESSED.", orderId);
    }

    @Test(groups = "integration", description = "Should route a malformed event to the dead-letter queue")
    public void testMalformedOrderGoesToDlq() {
        String orderId = UUID.randomUUID().toString();
        String invalidOrderJson = "{\"id\":\"" + orderId + "\"}";

        try (TestKafkaConsumer dlqConsumer = new TestKafkaConsumer(kafka.getBootstrapServers(), DLQ_TOPIC, orderId)) {

            LOGGER.info("Sending malformed order {} to Kafka", orderId);
            testKafkaProducer.sendRawJson(orderId, invalidOrderJson);

            LOGGER.info("Awaiting for DLQ to receive message for order {}", orderId);
            Awaitility.await()
                    .atMost(Duration.ofSeconds(20))
                    .pollInterval(Duration.ofMillis(500))
                    .untilAsserted(() -> {
                        Optional<String> receivedMessage = dlqConsumer.pollForMessageWithKey();
                        assertTrue(receivedMessage.isPresent(), "Message for order " + orderId + " should be in DLQ");
                        assertTrue(receivedMessage.get().contains(orderId));
                    });

            LOGGER.info("Successfully verified malformed order {} was sent to DLQ.", orderId);
        }
    }

    private static class TestKafkaConsumer implements AutoCloseable {
        private final KafkaConsumer<String, String> consumer;
        private final String messageKeyToFind;

        public TestKafkaConsumer(String bootstrapServers, String topic, String messageKeyToFind) {
            this.messageKeyToFind = messageKeyToFind;
            Properties props = new Properties();
            props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
            props.put(ConsumerConfig.GROUP_ID_CONFIG, "test-consumer-" + UUID.randomUUID());
            props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());
            props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());
            props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
            this.consumer = new KafkaConsumer<>(props);
            this.consumer.subscribe(Collections.singletonList(topic));
        }

        public Optional<String> pollForMessageWithKey() {
            ConsumerRecords<String, String> records = consumer.poll(Duration.ofMillis(1000));
            for (ConsumerRecord<String, String> record : records) {
                if (messageKeyToFind.equals(record.key())) {
                    return Optional.of(record.value());
                }
            }
            return Optional.empty();
        }

        @Override
        public void close() {
            try {
                // Any additional cleanup logic if needed
            } finally {
                if (consumer != null) {
                    consumer.close();
                }
            }
        }
    }
}