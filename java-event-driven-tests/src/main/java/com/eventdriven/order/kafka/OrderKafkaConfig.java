package com.eventdriven.order.kafka;

import com.eventdriven.order.model.Order;
import org.apache.kafka.clients.admin.NewTopic;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.kafka.common.serialization.StringSerializer;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.kafka.annotation.EnableKafka;
import org.springframework.kafka.config.ConcurrentKafkaListenerContainerFactory;
import org.springframework.kafka.config.TopicBuilder;
import org.springframework.kafka.core.*;
import org.springframework.kafka.listener.ContainerProperties;
import org.springframework.kafka.listener.DefaultErrorHandler;
import org.springframework.kafka.support.serializer.ErrorHandlingDeserializer;
import org.springframework.kafka.support.serializer.JsonDeserializer;
import org.springframework.kafka.support.serializer.JsonSerializer;
import org.springframework.util.backoff.FixedBackOff;

import java.util.HashMap;
import java.util.Map;

/**
 * Enhanced Kafka configuration with manual acknowledgment, error handling, and
 * performance optimizations.
 */
@EnableKafka
@Configuration
public class OrderKafkaConfig {

    @Value("${spring.kafka.bootstrap-servers:localhost:9092}")
    private String bootstrapServers;

    @Value("${spring.kafka.consumer.group-id:order_group}")
    private String groupId;

    @Value("${spring.kafka.listener.concurrency:3}")
    private int concurrency;

    /**
     * Consumer factory with error handling deserializer and performance tuning.
     */
    @Bean
    public ConsumerFactory<String, Order> consumerFactory() {
        Map<String, Object> props = new HashMap<>();

        // Connection settings
        props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        props.put(ConsumerConfig.GROUP_ID_CONFIG, groupId);

        // Consumer behavior
        props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, false); // Manual ack

        // Performance tuning
        props.put(ConsumerConfig.MAX_POLL_RECORDS_CONFIG, 10);
        props.put(ConsumerConfig.MAX_POLL_INTERVAL_MS_CONFIG, 300000); // 5 minutes
        props.put(ConsumerConfig.SESSION_TIMEOUT_MS_CONFIG, 30000); // 30 seconds
        props.put(ConsumerConfig.HEARTBEAT_INTERVAL_MS_CONFIG, 10000); // 10 seconds

        // Deserialization with error handling
        props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, ErrorHandlingDeserializer.class);
        props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, ErrorHandlingDeserializer.class);
        props.put(ErrorHandlingDeserializer.KEY_DESERIALIZER_CLASS, StringDeserializer.class);
        props.put(ErrorHandlingDeserializer.VALUE_DESERIALIZER_CLASS, JsonDeserializer.class);

        // JSON deserializer configuration
        props.put(JsonDeserializer.TRUSTED_PACKAGES, "com.eventdriven.order.model");
        props.put(JsonDeserializer.VALUE_DEFAULT_TYPE, Order.class.getName());
        props.put(JsonDeserializer.USE_TYPE_INFO_HEADERS, false);

        return new DefaultKafkaConsumerFactory<>(props);
    }

    /**
     * Producer factory with performance optimizations and reliability settings.
     */
    @Bean
    public ProducerFactory<String, Order> producerFactory() {
        Map<String, Object> props = new HashMap<>();

        // Connection settings
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);

        // Serialization
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class);
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, JsonSerializer.class);

        // Reliability settings
        props.put(ProducerConfig.ACKS_CONFIG, "all");
        props.put(ProducerConfig.RETRIES_CONFIG, 3);
        props.put(ProducerConfig.MAX_IN_FLIGHT_REQUESTS_PER_CONNECTION, 5);
        props.put(ProducerConfig.ENABLE_IDEMPOTENCE_CONFIG, true);

        // Performance optimizations
        props.put(ProducerConfig.COMPRESSION_TYPE_CONFIG, "snappy");
        props.put(ProducerConfig.LINGER_MS_CONFIG, 100);
        props.put(ProducerConfig.BATCH_SIZE_CONFIG, 16384);
        props.put(ProducerConfig.BUFFER_MEMORY_CONFIG, 33554432); // 32MB

        return new DefaultKafkaProducerFactory<>(props);
    }

    /**
     * Kafka template for sending messages.
     */
    @Bean
    public KafkaTemplate<String, Order> kafkaTemplate() {
        KafkaTemplate<String, Order> template = new KafkaTemplate<>(producerFactory());
        template.setDefaultTopic("order_created_topic");
        return template;
    }

    /**
     * Listener container factory with manual acknowledgment and error handling.
     */
    @Bean
    public ConcurrentKafkaListenerContainerFactory<String, Order> orderKafkaListenerContainerFactory() {
        ConcurrentKafkaListenerContainerFactory<String, Order> factory = new ConcurrentKafkaListenerContainerFactory<>();

        factory.setConsumerFactory(consumerFactory());
        factory.setConcurrency(concurrency);

        // Enable manual acknowledgment
        factory.getContainerProperties().setAckMode(ContainerProperties.AckMode.MANUAL);

        // Configure error handling
        DefaultErrorHandler errorHandler = new DefaultErrorHandler(
                new FixedBackOff(1000L, 3)); // 3 retries with 1 second delay
        errorHandler.addNotRetryableExceptions(IllegalArgumentException.class);
        factory.setCommonErrorHandler(errorHandler);

        // Enable batch listening for better performance (optional)
        factory.setBatchListener(false);

        // Set poll timeout
        factory.getContainerProperties().setPollTimeout(3000);

        return factory;
    }

    /**
     * Create order_created_topic with proper configuration.
     */
    @Bean
    public NewTopic orderCreatedTopic() {
        return TopicBuilder.name("order_created_topic")
                .partitions(3)
                .replicas(1)
                .config("cleanup.policy", "delete")
                .config("retention.ms", "604800000") // 7 days
                .config("segment.ms", "86400000") // 1 day
                .config("compression.type", "snappy")
                .build();
    }

    /**
     * Create dead letter topic with longer retention.
     */
    @Bean
    public NewTopic deadLetterTopic() {
        return TopicBuilder.name("order_dead_letter_topic")
                .partitions(1)
                .replicas(1)
                .config("cleanup.policy", "delete")
                .config("retention.ms", "2592000000") // 30 days
                .config("compression.type", "gzip") // Better compression for long-term storage
                .build();
    }
}