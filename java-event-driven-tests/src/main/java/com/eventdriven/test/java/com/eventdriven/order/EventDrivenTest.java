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
import org.testcontainers.utility.DockerImageName;
import org.testcontainers.images.builder.ImageFromDockerfile;
import org.testcontainers.containers.wait.strategy.Wait;
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
 * application in Docker containers, and verifies end-to-end event processing
 * and error
 * handling.
 *
 * Tests the complete flow:
 * 1. Kafka receives order events
 * 2. Spring Boot application consumes events
 * 3. Order status is updated in Redis
 * 4. Failed events are sent to dead letter queue
 */
@Test(groups = "integration")
public class EventDrivenTest {

    private static final Logger LOGGER = LoggerFactory.getLogger(EventDrivenTest.class);

    private static final String TOPIC = "order_created_topic";
    private static final String DLQ_TOPIC = "order_dead_letter_topic";
    private static final String KAFKA_IMAGE = "apache/kafka:3.8.1";
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
    private static final Duration HEALTH_TIMEOUT = Duration.ofMinutes(3);

    private static final Network network = Network.newNetwork();

    private static KafkaContainer kafka;
    private static GenericContainer<?> redis;
    private static GenericContainer<?> orderServiceApp;

    private static Jedis jedis;
    private static TestKafkaProducer testKafkaProducer;

    static {
        LOGGER.info("=== EventDrivenTest CLASS LOADED ===");
    }

    @BeforeClass(groups = "integration")
    public static void setupTestClass() {
        LOGGER.info("=== Starting EventDrivenTest Setup ===");

        // Check Docker availability first
        if (!DockerClientFactory.instance().isDockerAvailable()) {
            String message = "Docker is not available. Please start Docker Desktop and try again.";
            LOGGER.error(message);
            throw new SkipException(message);
        }

        LOGGER.info("Docker is available, proceeding with container setup...");

        try {
            // Start infrastructure containers first
            LOGGER.info("Starting Kafka container...");
            kafka = new KafkaContainer(DockerImageName.parse(KAFKA_IMAGE))
                    .withNetwork(network)
                    .withNetworkAliases(KAFKA_ALIAS);
            kafka.start();
            LOGGER.info("Kafka started successfully on: {}", kafka.getBootstrapServers());

            LOGGER.info("Starting Redis container...");
            redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                    .withExposedPorts(REDIS_PORT)
                    .withNetwork(network)
                    .withNetworkAliases(REDIS_ALIAS);
            redis.start();
            LOGGER.info("Redis started successfully on: {}:{}", redis.getHost(), redis.getFirstMappedPort());

            // Build and start the application container
            LOGGER.info("Building application Docker image...");
            orderServiceApp = new GenericContainer<>(
                    new ImageFromDockerfile()
                            .withDockerfile(Paths.get("Dockerfile")))
                    .withExposedPorts(ORDER_SERVICE_PORT)
                    .withNetwork(network)
                    .dependsOn(kafka, redis)
                    .withEnv(ENV_KAFKA_BOOTSTRAP, KAFKA_ALIAS + ":9092")
                    .withEnv(ENV_REDIS_HOST, REDIS_ALIAS)
                    .withEnv(ENV_MANAGEMENT_ENDPOINTS, MANAGEMENT_ENDPOINTS_VALUE)
                    .waitingFor(Wait.forHttp(HEALTH_ENDPOINT)
                            .forStatusCode(200)
                            .withStartupTimeout(HEALTH_TIMEOUT))
                    .withLogConsumer(outputFrame -> LOGGER.debug("APP: {}", outputFrame.getUtf8String()));

            LOGGER.info("Starting application container...");
            orderServiceApp.start();
            LOGGER.info("Application started successfully");

            // Setup test clients
            setupTestClients();

            LOGGER.info("=== EventDrivenTest Setup Complete ===");

        } catch (Exception e) {
            LOGGER.error("Failed to start containers during setup", e);
            cleanupContainersAndResources();
            throw new RuntimeException("Failed to start containers: " + e.getMessage(), e);
        }
    }

    private static void setupTestClients() {
        try {
            // Setup Redis client
            LOGGER.info("Setting up Redis client...");
            jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
            jedis.ping(); // Test connection
            LOGGER.info("Redis client connected successfully");

            // Setup Kafka producer
            LOGGER.info("Setting up Kafka producer...");
            testKafkaProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);
            LOGGER.info("Kafka producer created successfully");

        } catch (Exception e) {
            LOGGER.error("Failed to setup test clients", e);
            throw new RuntimeException("Failed to setup test clients: " + e.getMessage(), e);
        }
    }

    @AfterClass(groups = "integration")
    public static void cleanupTestClass() {
        LOGGER.info("=== Starting EventDrivenTest Cleanup ===");
        cleanupContainersAndResources();
        LOGGER.info("=== EventDrivenTest Cleanup Complete ===");
    }

    private static void cleanupContainersAndResources() {
        // Close test clients
        if (jedis != null) {
            try {
                jedis.close();
                LOGGER.info("Redis client closed");
            } catch (Exception e) {
                LOGGER.error("Failed to close Jedis", e);
            }
        }

        if (testKafkaProducer != null) {
            try {
                testKafkaProducer.close();
                LOGGER.info("Kafka producer closed");
            } catch (Exception e) {
                LOGGER.error("Failed to close TestKafkaProducer", e);
            }
        }

        // Stop containers in reverse order of startup
        if (orderServiceApp != null) {
            try {
                orderServiceApp.stop();
                LOGGER.info("Application container stopped");
            } catch (Exception e) {
                LOGGER.error("Failed to stop application container", e);
            }
        }

        if (redis != null) {
            try {
                redis.stop();
                LOGGER.info("Redis container stopped");
            } catch (Exception e) {
                LOGGER.error("Failed to stop Redis container", e);
            }
        }

        if (kafka != null) {
            try {
                kafka.stop();
                LOGGER.info("Kafka container stopped");
            } catch (Exception e) {
                LOGGER.error("Failed to stop Kafka container", e);
            }
        }

        if (network != null) {
            try {
                network.close();
                LOGGER.info("Network closed");
            } catch (Exception e) {
                LOGGER.error("Failed to close network", e);
            }
        }
    }

    @BeforeMethod(groups = "integration")
    public void setupTestMethod() {
        LOGGER.debug("=== Setting up test method ===");

        // Verify containers are still running
        if (kafka == null || redis == null || jedis == null || testKafkaProducer == null || orderServiceApp == null) {
            throw new IllegalStateException("Test infrastructure not properly initialized. " +
                    "Kafka: " + (kafka != null) + ", Redis: " + (redis != null) +
                    ", Jedis: " + (jedis != null) + ", Producer: " + (testKafkaProducer != null) +
                    ", App: " + (orderServiceApp != null));
        }

        // Verify Redis connection and reconnect if needed
        try {
            jedis.ping(); // Test connection first
            LOGGER.debug("Redis connection verified");
        } catch (Exception e) {
            LOGGER.warn("Redis connection lost, reconnecting...");
            try {
                if (jedis != null) {
                    jedis.close();
                }
                jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
                jedis.ping();
                LOGGER.info("Redis reconnected successfully");
            } catch (Exception reconnectException) {
                LOGGER.error("Failed to reconnect to Redis", reconnectException);
                throw new RuntimeException("Redis connection failed: " + reconnectException.getMessage(),
                        reconnectException);
            }
        }

        try {
            jedis.flushDB();
            LOGGER.debug("Redis database flushed for new test");
        } catch (Exception e) {
            LOGGER.warn("Failed to flush Redis: {}", e.getMessage());
            // Don't fail the test for flush issues, just log it
            LOGGER.debug("Continuing test despite flush failure...");
        }
    }

    @Test(groups = "integration", description = "Should process an order event end-to-end: Kafka → Spring Boot → Redis")
    public void testOrderEventEndToEnd() {
        LOGGER.info("=== Starting testOrderEventEndToEnd ===");

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

        // Wait for the Spring Boot application to consume the message and update Redis
        Awaitility.await()
                .atMost(Duration.ofSeconds(30))
                .pollInterval(Duration.ofMillis(500))
                .untilAsserted(() -> {
                    try {
                        String status = jedis.get(redisKey);
                        LOGGER.debug("Polling Redis for order {}. Current status: {}", orderId, status);
                        assertEquals(status, "PROCESSED",
                                "Order status in Redis should be 'PROCESSED' for order " + orderId);
                    } catch (JedisConnectionException e) {
                        fail("Redis connection lost during test: " + e.getMessage());
                    }
                });

        LOGGER.info("Successfully verified order {} is PROCESSED in Redis!", orderId);
        LOGGER.info("✅ End-to-end test passed: Kafka → Spring Boot Consumer → Redis");
    }

    @Test(groups = "integration", description = "Should route a malformed event to the dead-letter queue")
    public void testMalformedOrderGoesToDlq() {
        LOGGER.info("=== Starting testMalformedOrderGoesToDlq ===");

        String orderId = UUID.randomUUID().toString();
        // Missing required 'description' field to trigger deserialization error
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

            LOGGER.info("Successfully verified malformed order {} was sent to DLQ!", orderId);
            LOGGER.info("✅ Dead Letter Queue test passed: Malformed message → DLQ");
        }
    }

    /**
     * Test helper class for consuming messages from Kafka topics
     */
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
                    LOGGER.error("Failed to close consumer gracefully", e);
                }
            }
        }
    }
}