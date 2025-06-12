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
import org.testcontainers.containers.KafkaContainer;
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

@Test
public class EventDrivenTest {

    private static final Logger LOGGER = LoggerFactory.getLogger(EventDrivenTest.class);
    private static final String TOPIC = "order_created_topic";
    private static final String DLQ_TOPIC = "order_dead_letter_topic";
    private static final Network network = Network.newNetwork();

    private KafkaContainer kafka;
    private GenericContainer<?> redis;
    private GenericContainer<?> orderServiceApp;

    private Jedis jedis;
    private TestKafkaProducer testKafkaProducer;

    @BeforeClass
    public void setupTestClass() {
        kafka = new KafkaContainer(DockerImageName.parse("confluentinc/cp-kafka:7.2.1"))
                .withNetwork(network)
                .withNetworkAliases("kafka");

        redis = new GenericContainer<>(DockerImageName.parse("redis:7.0.2"))
                .withExposedPorts(6379)
                .withNetwork(network)
                .withNetworkAliases("redis");

        orderServiceApp = new GenericContainer<>(
                new ImageFromDockerfile().withDockerfile(Paths.get("Dockerfile")))
                .withExposedPorts(8080)
                .withNetwork(network)
                .dependsOn(kafka, redis)
                .withEnv("SPRING_KAFKA_BOOTSTRAP_SERVERS", "kafka:9092")
                .withEnv("SPRING_DATA_REDIS_HOST", "redis")
                .withEnv("MANAGEMENT_ENDPOINTS_WEB_EXPOSURE_INCLUDE", "health")
                .waitingFor(Wait.forHttp("/actuator/health").forStatusCode(200)
                        .withStartupTimeout(Duration.ofSeconds(120)));

        LOGGER.info("Starting Testcontainers... This will take a moment.");
        kafka.start();
        redis.start();
        orderServiceApp.start();
        LOGGER.info("Testcontainers started successfully.");

        jedis = new Jedis(redis.getHost(), redis.getFirstMappedPort());
        testKafkaProducer = new TestKafkaProducer(kafka.getBootstrapServers(), TOPIC);
    }

    @AfterClass
    public void cleanupTestClass() {
        LOGGER.info("Stopping Testcontainers...");
        if (jedis != null)
            jedis.close();
        if (testKafkaProducer != null)
            testKafkaProducer.close();
        if (orderServiceApp != null)
            orderServiceApp.stop();
        if (redis != null)
            redis.stop();
        if (kafka != null)
            kafka.stop();
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
        // This JSON is malformed because the 'description' field is missing, which the
        // service expects.
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
                        // Optionally, assert the content of the received message
                        assertTrue(receivedMessage.get().contains(orderId));
                    });

            LOGGER.info("Successfully verified malformed order {} was sent to DLQ.", orderId);
        }
    }

    // CLASS to consume from Kafka topics within tests
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
            ConsumerRecords<String, String> records = consumer.poll(Duration.ofMillis(200));
            for (ConsumerRecord<String, String> record : records) {
                if (messageKeyToFind.equals(record.key())) {
                    return Optional.of(record.value());
                }
            }
            return Optional.empty();
        }

        @Override
        public void close() {
            if (consumer != null) {
                consumer.close();
            }
        }
    }
}