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
import org.testng.annotations.AfterClass;
import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

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
 * This class uses Testcontainers to spin up Kafka and Redis containers,
 * and verifies end-to-end event processing and error handling.
 */
@Test
public class EventDrivenTest {

    private static final Logger LOGGER = LoggerFactory.getLogger(EventDrivenTest.class);

    private static final String TOPIC = "order_created_topic";
    private static final String DLQ_TOPIC = "order_dead_letter_topic";
    private static final String KAFKA_IMAGE = "confluentinc/cp-kafka:7.7.0";
    private static final String REDIS_IMAGE = "redis:7-alpine";
    private static final int REDIS_PORT = 6379;

    private static final Network network = Network.newNetwork();

    private static KafkaContainer kafka;
    private static GenericContainer<?> redis;

    private static Jedis jedis;
    private static TestKafkaProducer testKafkaProducer;

    @BeforeClass
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
            // Start Kafka container
            LOGGER.info("Starting Kafka container...");
            kafka = new KafkaContainer(DockerImageName.parse(KAFKA_IMAGE))
                    .withNetwork(network)
                    .withNetworkAliases("kafka");
            kafka.start();
            LOGGER.info("Kafka started successfully on: {}", kafka.getBootstrapServers());

            // Start Redis container
            LOGGER.info("Starting Redis container...");
            redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                    .withExposedPorts(REDIS_PORT)
                    .withNetwork(network)
                    .withNetworkAliases("redis");
            redis.start();
            LOGGER.info("Redis started successfully on: {}:{}", redis.getHost(), redis.getFirstMappedPort());

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

    @AfterClass
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
                LOGGER.warn("Failed to close Jedis", e);
            }
        }

        if (testKafkaProducer != null) {
            try {
                testKafkaProducer.close();
                LOGGER.info("Kafka producer closed");
            } catch (Exception e) {
                LOGGER.warn("Failed to close TestKafkaProducer", e);
            }
        }

        // Stop containers
        if (redis != null) {
            try {
                redis.stop();
                LOGGER.info("Redis container stopped");
            } catch (Exception e) {
                LOGGER.warn("Failed to stop Redis container", e);
            }
        }

        if (kafka != null) {
            try {
                kafka.stop();
                LOGGER.info("Kafka container stopped");
            } catch (Exception e) {
                LOGGER.warn("Failed to stop Kafka container", e);
            }
        }

        if (network != null) {
            try {
                network.close();
                LOGGER.info("Network closed");
            } catch (Exception e) {
                LOGGER.warn("Failed to close network", e);
            }
        }
    }

    @BeforeMethod
    public void setupTestMethod() {
        // Verify containers are still running
        if (kafka == null || redis == null || jedis == null || testKafkaProducer == null) {
            throw new IllegalStateException("Test infrastructure not properly initialized. " +
                    "Kafka: " + (kafka != null) + ", Redis: " + (redis != null) +
                    ", Jedis: " + (jedis != null) + ", Producer: " + (testKafkaProducer != null));
        }

        try {
            jedis.flushDB();
            LOGGER.info("Redis database flushed for new test");
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