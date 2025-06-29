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
import org.testcontainers.DockerClientFactory;
import org.testng.SkipException;
import org.testcontainers.containers.Network;
import org.testcontainers.containers.wait.strategy.Wait;
import org.testcontainers.utility.DockerImageName;
import org.testcontainers.images.builder.ImageFromDockerfile;
import org.testng.annotations.AfterClass;
import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

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

/**
 * Integration tests for the event-driven order processing system.
 * <p>
 * This class uses Testcontainers to spin up Kafka, Redis, and the Order Service
 * application
 * in Docker containers, and verifies end-to-end event processing and error
 * handling.
 */

@Test
public class EventDrivenTest {

    private static final Logger LOGGER = LoggerFactory.getLogger(EventDrivenTest.class);

    private static final String TOPIC = "order_created_topic";
    private static final String DLQ_TOPIC = "order_dead_letter_topic";
    private static final String KAFKA_IMAGE = "confluentinc/cp-kafka:7.7.0";
    private static final String REDIS_IMAGE = "redis:7-alpine";
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

    private static final Network network = Network.newNetwork();

    private static KafkaContainer kafka;
    private static GenericContainer<?> redis;
    private static GenericContainer<?> orderServiceApp;

    private static Jedis jedis;
    private static TestKafkaProducer testKafkaProducer;

    @BeforeClass
    public static void setupTestClass() {
        if (!DockerClientFactory.instance().isDockerAvailable()) {
            throw new SkipException("Docker is not available, skipping integration tests");
        }
        try {
            kafka = new KafkaContainer(DockerImageName.parse(KAFKA_IMAGE))
                    .withNetwork(network)
                    .withNetworkAliases(KAFKA_ALIAS)
                    .withEmbeddedZookeeper();

            redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                    .withExposedPorts(REDIS_PORT)
                    .withNetwork(network)
                    .withNetworkAliases(REDIS_ALIAS);

            // Start infrastructure containers first
            LOGGER.info("Starting Kafka container...");
            kafka.start();
            LOGGER.info("Kafka started on: {}", kafka.getBootstrapServers());

            LOGGER.info("Starting Redis container...");
            redis.start();
            LOGGER.info("Redis started on: {}:{}", redis.getHost(), redis.getFirstMappedPort());

            // Build and start the application container
            LOGGER.info("Building application Docker image...");
            orderServiceApp = new GenericContainer<>(
                    new ImageFromDockerfile()
                            .withDockerfile(Paths.get("Dockerfile"))
                            .withBuildArg("SKIP_TESTS", "true"))
                    .withExposedPorts(ORDER_SERVICE_PORT)
                    .withNetwork(network)
                    .dependsOn(kafka, redis)
                    .withEnv(ENV_KAFKA_BOOTSTRAP, KAFKA_ALIAS + ":9092")
                    .withEnv(ENV_REDIS_HOST, REDIS_ALIAS)
                    .withEnv(ENV_MANAGEMENT_ENDPOINTS, MANAGEMENT_ENDPOINTS_VALUE)
                    .waitingFor(Wait.forHttp(HEALTH_ENDPOINT)
                            .forStatusCode(200)
                            .withStartupTimeout(HEALTH_TIMEOUT))
                    .withLogConsumer(outputFrame -> LOGGER.info("APP: {}", outputFrame.getUtf8String()));

            LOGGER.info("Starting application container...");
            orderServiceApp.start();
            LOGGER.info("Application started successfully");

            // Setup test clients
            setupTestClients();

        } catch (Exception e) {
            LOGGER.error("Failed to start containers: {}", e.getMessage(), e);
            cleanupContainers();
            throw new RuntimeException("Failed to start containers", e);
        }
    }

    private static void setupTestClients() {
        Jedis tempJedis = null;
        TestKafkaProducer tempProducer = null;
        try {
            tempJedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
            tempJedis.ping();
            LOGGER.info("Redis connection established");

            tempProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);
            LOGGER.info("Kafka producer created");

            jedis = tempJedis;
            testKafkaProducer = tempProducer;
        } catch (Exception e) {
            LOGGER.error("Failed to setup test clients", e);
            if (tempProducer != null) {
                try {
                    tempProducer.close();
                } catch (Exception ex) {
                    LOGGER.warn("Failed to close Kafka producer during setup", ex);
                }
            }
            if (tempJedis != null) {
                try {
                    tempJedis.close();
                } catch (Exception ex) {
                    LOGGER.warn("Failed to close Jedis during setup", ex);
                }
            }
            throw new RuntimeException("Failed to setup test clients", e);
        }
    }

    private static void cleanupContainers() {
        if (orderServiceApp != null && orderServiceApp.isRunning()) {
            try {
                orderServiceApp.stop();
            } catch (Exception e) {
                LOGGER.warn("Failed to stop application container", e);
            }
        }
        if (redis != null && redis.isRunning()) {
            try {
                redis.stop();
            } catch (Exception e) {
                LOGGER.warn("Failed to stop Redis container", e);
            }
        }
        if (kafka != null && kafka.isRunning()) {
            try {
                kafka.stop();
            } catch (Exception e) {
                LOGGER.warn("Failed to stop Kafka container", e);
            }
        }
        if (network != null) {
            try {
                network.close();
            } catch (Exception e) {
                LOGGER.warn("Failed to close network", e);
            }
        }
    }

    @AfterClass
    public static void cleanupTestClass() {
        LOGGER.info("Stopping Testcontainers...");
        try {
            if (jedis != null) {
                jedis.close();
            }
        } catch (Exception e) {
            LOGGER.warn("Failed to close Jedis", e);
        }
        try {
            if (testKafkaProducer != null) {
                testKafkaProducer.close();
            }
        } catch (Exception e) {
            LOGGER.warn("Failed to close TestKafkaProducer", e);
        }
        cleanupContainers();
        LOGGER.info("Testcontainers stopped.");
    }

    @BeforeMethod
    public void setupTestMethod() {
        try {
            jedis.flushDB();
            LOGGER.info("Redis database flushed");
        } catch (JedisConnectionException e) {
            LOGGER.error("Failed to flush Redis", e);
            fail("Redis connection failed: " + e.getMessage());
        }
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

        String redisKey = REDIS_KEY_PREFIX + orderId;
        LOGGER.info("Awaiting for Redis key {} to be PROCESSED...", redisKey);

        Awaitility.await()
                .atMost(Duration.ofSeconds(30))
                .pollInterval(Duration.ofMillis(500))
                .untilAsserted(() -> {
                    try {
                        String status = jedis.get(redisKey);
                        LOGGER.info("Polling Redis for order {}. Current status: {}", orderId, status);
                        assertEquals(status, "PROCESSED",
                                "Order status in Redis should be 'PROCESSED' for order " + orderId);
                    } catch (JedisConnectionException e) {
                        fail("Redis connection lost during test: " + e.getMessage());
                    }
                });

        LOGGER.info("Successfully verified order {} is PROCESSED.", orderId);
    }

    @Test(groups = "integration", description = "Should route a malformed event to the dead-letter queue")
    public void testMalformedOrderGoesToDlq() {
        String orderId = UUID.randomUUID().toString();
        // Missing required 'description' field
        String invalidOrderJson = "{\"id\":\"" + orderId + "\"}";

        try (TestKafkaConsumer dlqConsumer = new TestKafkaConsumer(kafka.getBootstrapServers(), DLQ_TOPIC, orderId)) {

            LOGGER.info("Sending malformed order {} to Kafka", orderId);
            testKafkaProducer.sendRawJson(orderId, invalidOrderJson);

            LOGGER.info("Awaiting for DLQ to receive message for order {}", orderId);
            Awaitility.await()
                    .atMost(Duration.ofSeconds(30))
                    .pollInterval(Duration.ofMillis(500))
                    .untilAsserted(() -> {
                        Optional<String> receivedMessage = dlqConsumer.pollForMessageWithKey();
                        assertTrue(receivedMessage.isPresent(), 
                            "Message for order " + orderId + " should be in DLQ");
                        assertTrue(receivedMessage.get().contains(orderId),
                            "DLQ message should contain order ID");
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
            props.put(ConsumerConfig.MAX_POLL_RECORDS_CONFIG, "50");
            this.consumer = new KafkaConsumer<>(props);
            this.consumer.subscribe(Collections.singletonList(topic));
        }

        public Optional<String> pollForMessageWithKey() {
            ConsumerRecords<String, String> records = consumer.poll(Duration.ofMillis(1000));
            for (ConsumerRecord<String, String> record : records) {
                LOGGER.debug("Received record with key: {}", record.key());
                if (messageKeyToFind.equals(record.key())) {
                    return Optional.of(record.value());
                }
            }
            return Optional.empty();
        }

        @Override
        public void close() {
            if (consumer != null) {
                try {
                    consumer.close(Duration.ofSeconds(5));
                } catch (Exception e) {
                    LOGGER.warn("Failed to close consumer gracefully", e);
                }
            }
        }
    }
}

