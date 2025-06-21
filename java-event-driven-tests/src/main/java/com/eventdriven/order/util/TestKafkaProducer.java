package com.eventdriven.order.util;

import com.eventdriven.order.model.Order;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.apache.kafka.clients.producer.*;
import org.apache.kafka.common.serialization.StringSerializer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import java.util.Objects;
import java.util.Properties;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

/**
 * Thread-safe test utility for sending messages to Kafka in integration tests.
 * <p>
 * This class is thread-safe: multiple threads can share a single instance.
 */
public class TestKafkaProducer implements AutoCloseable {

    private static final Logger logger = LoggerFactory.getLogger(TestKafkaProducer.class);
    private static final ObjectMapper objectMapper = new ObjectMapper();
    /** Timeout in seconds for Kafka send acknowledgements. */
    private static final long SEND_TIMEOUT_SECONDS = 10L;
    private final KafkaProducer<String, String> producer;
    private final String topic;

    /**
     * Constructs a test Kafka producer for the given bootstrap servers and topic.
     * @param bootstrapServers Kafka bootstrap servers
     * @param topic Kafka topic to send messages to
     */
    public TestKafkaProducer(String bootstrapServers, String topic) {
        Objects.requireNonNull(bootstrapServers, "bootstrapServers must not be null");
        Objects.requireNonNull(topic, "topic must not be null");
        if (topic.isBlank()) {
            throw new IllegalArgumentException("topic must not be blank");
        }
        this.topic = topic;
        Properties props = new Properties();
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.ACKS_CONFIG, "all");
        props.put(ProducerConfig.RETRIES_CONFIG, 3);
        this.producer = new KafkaProducer<>(props);
    }

    /**
     * Sends an {@link Order} as a JSON message to the configured Kafka topic.
     *
     * @param order the order to send (must not be null)
     * @throws JsonProcessingException if serialization fails
     * @throws RuntimeException if sending to Kafka fails
     */
    public void sendOrder(Order order) throws JsonProcessingException {
        Objects.requireNonNull(order, "order must not be null");
        String orderJson = objectMapper.writeValueAsString(order);
        ProducerRecord<String, String> record = new ProducerRecord<>(topic, order.getId(), orderJson);
        sendRecord(record, "order " + order.getId());
    }

    /**
     * Sends a raw JSON string value to the topic.
     * This is used to simulate malformed events to test DLQ functionality.
     *
     * @param key The message key.
     * @param jsonValue The raw JSON string to send.
     * @throws RuntimeException if sending to Kafka fails
     */
    public void sendRawJson(String key, String jsonValue) {
        ProducerRecord<String, String> record = new ProducerRecord<>(topic, key, jsonValue);
        sendRecord(record, "raw JSON with key " + key);
    }

    /**
     * Internal helper to send a record with timeout handling and logging.
     */
    private void sendRecord(ProducerRecord<String, String> record, String description) {
        try {
            // Wait for broker acknowledgement to avoid tests hanging indefinitely
            producer.send(record).get(SEND_TIMEOUT_SECONDS, TimeUnit.SECONDS);
            logger.info("Sent {} to Kafka", description);
        } catch (TimeoutException te) {
            logger.error("Timed out after {} seconds sending {}", SEND_TIMEOUT_SECONDS, description, te);
            throw new RuntimeException("Timed out after " + SEND_TIMEOUT_SECONDS + " seconds sending " + description + " to Kafka", te);
        } catch (Exception e) {
            logger.error("Failed to send {}", description, e);
            throw new RuntimeException("Failed to send " + description + " to Kafka", e);
        }
        producer.flush();
    }

    /**
     * Closes the Kafka producer and releases resources.
     */
    @Override
    public void close() {
        producer.close();
    }
}
