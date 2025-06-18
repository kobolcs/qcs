package com.eventdriven.order.util;

import com.eventdriven.order.model.Order;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.apache.kafka.clients.producer.*;
import org.apache.kafka.common.serialization.StringSerializer;
import java.util.Properties;

public class TestKafkaProducer implements AutoCloseable {

    private final KafkaProducer<String, String> producer;
    private final String topic;
    private final ObjectMapper objectMapper = new ObjectMapper();

    public TestKafkaProducer(String bootstrapServers, String topic) {
        this.topic = topic;
        Properties props = new Properties();
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.ACKS_CONFIG, "all");
        props.put(ProducerConfig.RETRIES_CONFIG, 3);
        this.producer = new KafkaProducer<>(props);
    }

    public void sendOrder(Order order) throws JsonProcessingException {
        String orderJson = objectMapper.writeValueAsString(order);
        ProducerRecord<String, String> record = new ProducerRecord<>(topic, order.getId(), orderJson);
        try {
            producer.send(record).get(); // NOTE: wait for ack so tests know the send succeeded
            System.out.println("Sent order to Kafka: " + order.getId());
        } catch (Exception e) {
            System.err.println("Failed to send order to Kafka: " + order.getId());
            e.printStackTrace();
        }
        producer.flush();
    }

    /**
     *  Sends a raw JSON string value to the topic.
     * This is used to simulate malformed events to test DLQ functionality.
     *
     * @param key       The message key.
     * @param jsonValue The raw JSON string to send.
     */
    public void sendRawJson(String key, String jsonValue) {
        ProducerRecord<String, String> record = new ProducerRecord<>(topic, key, jsonValue);
        try {
            producer.send(record).get(); // NOTE: wait for ack so tests know the send succeeded
            System.out.println("Sent raw JSON to Kafka with key: " + key);
        } catch (Exception e) {
            System.err.println("Failed to send raw JSON to Kafka with key: " + key);
            e.printStackTrace();
        }
        producer.flush();
    }

    @Override
    public void close() {
        producer.close();
    }
}
