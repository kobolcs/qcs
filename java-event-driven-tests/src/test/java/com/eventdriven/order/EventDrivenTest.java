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
 */
@Test(groups = "integration")
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

    static {
        System.out.println("=== EventDrivenTest CLASS LOADED ===");
    }

    @BeforeClass(groups = "integration")
    public static void setupTestClass() {
        System.out.println("=== Starting EventDrivenTest Setup ===");

        // Check Docker availability first
        if (!DockerClientFactory.instance().isDockerAvailable()) {
            String message = "Docker is not available. Please start Docker Desktop and try again.";
            LOGGER.error(message);
            throw new SkipException(message);
        }

        System.out.println("Docker is available, proceeding with container setup...");

        try {
            // Start Kafka container
            System.out.println("Starting Kafka container...");
            kafka = new KafkaContainer(DockerImageName.parse(KAFKA_IMAGE))
                    .withNetwork(network)
                    .withNetworkAliases("kafka");
            kafka.start();
            System.out.println("Kafka started successfully on: " + kafka.getBootstrapServers());

            // Start Redis container
            System.out.println("Starting Redis container...");
            redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                    .withExposedPorts(REDIS_PORT)
                    .withNetwork(network)
                    .withNetworkAliases("redis");
            redis.start();
            System.out.println("Redis started successfully on: " + redis.getHost() + ":" + redis.getFirstMappedPort());

            // Setup test clients
            setupTestClients();

            System.out.println("=== EventDrivenTest Setup Complete ===");

        } catch (Exception e) {
            System.err.println("Failed to start containers during setup: " + e.getMessage());
            cleanupContainersAndResources();
            throw new RuntimeException("Failed to start containers: " + e.getMessage(), e);
        }
    }

    private static void setupTestClients() {
        try {
            // Setup Redis client
            System.out.println("Setting up Redis client...");
            jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
            jedis.ping(); // Test connection
            System.out.println("Redis client connected successfully");

            // Setup Kafka producer
            System.out.println("Setting up Kafka producer...");
            testKafkaProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);
            System.out.println("Kafka producer created successfully");

        } catch (Exception e) {
            System.err.println("Failed to setup test clients: " + e.getMessage());
            throw new RuntimeException("Failed to setup test clients: " + e.getMessage(), e);
        }
    }

    @AfterClass(groups = "integration")
    public static void cleanupTestClass() {
        System.out.println("=== Starting EventDrivenTest Cleanup ===");
        cleanupContainersAndResources();
        System.out.println("=== EventDrivenTest Cleanup Complete ===");
    }

    private static void cleanupContainersAndResources() {
        // Close test clients
        if (jedis != null) {
            try {
                jedis.close();
                System.out.println("Redis client closed");
            } catch (Exception e) {
                System.err.println("Failed to close Jedis: " + e.getMessage());
            }
        }

        if (testKafkaProducer != null) {
            try {
                testKafkaProducer.close();
                System.out.println("Kafka producer closed");
            } catch (Exception e) {
                System.err.println("Failed to close TestKafkaProducer: " + e.getMessage());
            }
        }

        // Stop containers
        if (redis != null) {
            try {
                redis.stop();
                System.out.println("Redis container stopped");
            } catch (Exception e) {
                System.err.println("Failed to stop Redis container: " + e.getMessage());
            }
        }

        if (kafka != null) {
            try {
                kafka.stop();
                System.out.println("Kafka container stopped");
            } catch (Exception e) {
                System.err.println("Failed to stop Kafka container: " + e.getMessage());
            }
        }

        if (network != null) {
            try {
                network.close();
                System.out.println("Network closed");
            } catch (Exception e) {
                System.err.println("Failed to close network: " + e.getMessage());
            }
        }
    }

@BeforeMethod(groups = "integration")
    public void setupTestMethod() {
        System.out.println("=== Setting up test mepackage com.eventdriven.order;

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
     */
    @Test(groups = "integration")
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

        static {
            System.out.println("=== EventDrivenTest CLASS LOADED ===");
        }

        @BeforeClass(groups = "integration")
        public static void setupTestClass() {
            System.out.println("=== Starting EventDrivenTest Setup ===");

            // Check Docker availability first
            if (!DockerClientFactory.instance().isDockerAvailable()) {
                String message = "Docker is not available. Please start Docker Desktop and try again.";
                LOGGER.error(message);
                throw new SkipException(message);
            }

            System.out.println("Docker is available, proceeding with container setup...");

            try {
                // Start Kafka container
                System.out.println("Starting Kafka container...");
                kafka = new KafkaContainer(DockerImageName.parse(KAFKA_IMAGE))
                        .withNetwork(network)
                        .withNetworkAliases("kafka");
                kafka.start();
                System.out.println("Kafka started successfully on: " + kafka.getBootstrapServers());

                // Start Redis container
                System.out.println("Starting Redis container...");
                redis = new GenericContainer<>(DockerImageName.parse(REDIS_IMAGE))
                        .withExposedPorts(REDIS_PORT)
                        .withNetwork(network)
                        .withNetworkAliases("redis");
                redis.start();
                System.out.println(
                        "Redis started successfully on: " + redis.getHost() + ":" + redis.getFirstMappedPort());

                // Setup test clients
                setupTestClients();

                System.out.println("=== EventDrivenTest Setup Complete ===");

            } catch (Exception e) {
                System.err.println("Failed to start containers during setup: " + e.getMessage());
                cleanupContainersAndResources();
                throw new RuntimeException("Failed to start containers: " + e.getMessage(), e);
            }
        }

        private static void setupTestClients() {
            try {
                // Setup Redis client
                System.out.println("Setting up Redis client...");
                jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
                jedis.ping(); // Test connection
                System.out.println("Redis client connected successfully");

                // Setup Kafka producer
                System.out.println("Setting up Kafka producer...");
                testKafkaProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);
                System.out.println("Kafka producer created successfully");

            } catch (Exception e) {
                System.err.println("Failed to setup test clients: " + e.getMessage());
                throw new RuntimeException("Failed to setup test clients: " + e.getMessage(), e);
            }
        }

        @AfterClass(groups = "integration")
        public static void cleanupTestClass() {
            System.out.println("=== Starting EventDrivenTest Cleanup ===");
            cleanupContainersAndResources();
            System.out.println("=== EventDrivenTest Cleanup Complete ===");
        }

        private static void cleanupContainersAndResources() {
        // Close test clients
        if (jedis != null) {
            try {
                jedis.close();
                System.out.println("Redis client closed");
            } catch (Exception e) {
                System.err.println("Failed to close Jedis: " + e.getMessage());
            }
        }

        if (testKafkaProducer != null) {
            try {
                testKafkaProducer.close();
                System.out.println("Kafka producer closed");
            } catch (Exception e) {
                System.err.println("Failed to close TestKafkaProducer: " + e.getMessage());
            }
        }

        // Stop containers
        if (redis != null) {
            try {
                redis.stop();
                System.out.println("Redis container stopped");
            } catch (Exception e) {
                System.err.println("Failed to stop Redis container: " + e.getMessage());
            }
        }

        if (kafka != null) {
            try {
                kafka.stop();
                System.out.println("Kafka container stopped");
            } catch (Exception e) {
                System.err.println("Failed to stop Kafka container: " + e.getMessage());
            }
        }

        if (network != null) {
            try {
                network.close();
                System.out.println("Network closed");
            } catch (Exception e) {
                System.err.println("Failed to close network: " + e.getMessage());
            }
        }
    }

    @BeforeMethod(groups = "integration")
    public void setupTestMethod() {
        System.out.println("=== Setting up test me