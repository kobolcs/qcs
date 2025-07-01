package com.eventdriven.order.kafka;

import com.eventdriven.order.model.Order;
import com.eventdriven.order.redis.OrderStatusRepository;
import io.github.resilience4j.circuitbreaker.annotation.CircuitBreaker;
import io.github.resilience4j.retry.annotation.Retry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.dao.DataAccessException;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.kafka.support.Acknowledgment;
import org.springframework.kafka.support.KafkaHeaders;
import org.springframework.messaging.handler.annotation.Header;
import org.springframework.messaging.handler.annotation.Payload;
import org.springframework.stereotype.Component;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.TimeUnit;

/**
 * Resilient consumer for order events from Kafka with retry logic and circuit
 * breaker pattern.
 * Processes orders and updates their status in Redis with proper error
 * handling.
 */
@Component
public class OrderEventConsumer {

    private static final Logger LOGGER = LoggerFactory.getLogger(OrderEventConsumer.class);
    private static final String STATUS_PROCESSED = "PROCESSED";
    private static final String STATUS_FAILED = "FAILED";
    private static final String DEAD_LETTER_TOPIC = "order_dead_letter_topic";
    private static final String CIRCUIT_BREAKER_NAME = "redis-cb";
    private static final String RETRY_NAME = "redis-retry";

    private final OrderStatusRepository orderStatusRepository;
    private final KafkaTemplate<String, Order> kafkaTemplate;

    @Autowired
    public OrderEventConsumer(OrderStatusRepository orderStatusRepository,
            KafkaTemplate<String, Order> kafkaTemplate) {
        this.orderStatusRepository = orderStatusRepository;
        this.kafkaTemplate = kafkaTemplate;
    }

    /**
     * Handles order created events from Kafka with manual acknowledgment.
     * Uses retry and circuit breaker patterns for resilience.
     */
    @KafkaListener(topics = "order_created_topic", groupId = "order_group", containerFactory = "orderKafkaListenerContainerFactory")
    public void handleOrderCreatedEvent(
            @Payload Order order,
            @Header(KafkaHeaders.RECEIVED_TOPIC) String topic,
            @Header(KafkaHeaders.RECEIVED_PARTITION) int partition,
            @Header(KafkaHeaders.OFFSET) long offset,
            Acknowledgment acknowledgment) {

        String orderId = order.getId();
        LOGGER.info("Received order event for ID: {} from topic: {}, partition: {}, offset: {}",
                orderId, topic, partition, offset);

        try {
            // Process the order with resilience patterns
            processOrderWithResilience(order);

            // Acknowledge the message only after successful processing
            acknowledgment.acknowledge();
            LOGGER.info("Successfully processed and acknowledged order ID: {}", orderId);

        } catch (Exception e) {
            LOGGER.error("Failed to process order ID: {} after all retries", orderId, e);

            // Send to DLQ asynchronously to avoid blocking
            sendToDeadLetterQueue(order, e)
                    .thenRun(acknowledgment::acknowledge)
                    .exceptionally(dlqError -> {
                        LOGGER.error("Critical: Failed to send order {} to DLQ", orderId, dlqError);
                        // Don't acknowledge - let Kafka retry
                        return null;
                    });
        }
    }

    @CircuitBreaker(name = CIRCUIT_BREAKER_NAME, fallbackMethod = "processOrderFallback")
    @Retry(name = RETRY_NAME)
    private void processOrderWithResilience(Order order) {
        String orderId = order.getId();

        try {
            // Validate order before processing
            validateOrder(order);

            // Update status in Redis
            orderStatusRepository.setStatus(orderId, STATUS_PROCESSED);
            LOGGER.debug("Set status to PROCESSED for order ID: {}", orderId);

            // Simulate any additional processing
            performAdditionalProcessing(order);

        } catch (JedisConnectionException | DataAccessException e) {
            // These exceptions will trigger retry and circuit breaker
            LOGGER.warn("Transient error processing order ID: {}", orderId, e);
            throw e;
        } catch (IllegalArgumentException e) {
            // Validation errors should not be retried
            LOGGER.error("Validation error for order ID: {}", orderId, e);
            throw new NonRetryableException("Order validation failed", e);
        }
    }

    /**
     * Fallback method called when circuit breaker is open
     */
    private void processOrderFallback(Order order, Exception e) {
        String orderId = order.getId();
        LOGGER.error("Circuit breaker activated - falling back for order ID: {}", orderId, e);

        // Store failed status if possible (might also fail if Redis is down)
        try {
            orderStatusRepository.setStatus(orderId, STATUS_FAILED);
        } catch (Exception fallbackError) {
            LOGGER.error("Fallback also failed for order ID: {}", orderId, fallbackError);
        }

        throw new RuntimeException("Order processing failed - circuit breaker open", e);
    }

    private void validateOrder(Order order) {
        if (order.getId() == null || order.getId().isEmpty()) {
            throw new IllegalArgumentException("Order ID cannot be null or empty");
        }

        // Add more validation as needed
        if (order.getDescription() != null && order.getDescription().length() > 1000) {
            throw new IllegalArgumentException("Order description too long");
        }
    }

    private void performAdditionalProcessing(Order order) {
        // Placeholder for additional business logic
        LOGGER.debug("Performing additional processing for order: {}", order.getId());
    }

    private CompletableFuture<Void> sendToDeadLetterQueue(Order order, Exception error) {
        return CompletableFuture.runAsync(() -> {
            try {
                // Add error metadata
                OrderWithError orderWithError = new OrderWithError(order, error.getMessage());

                kafkaTemplate.send(DEAD_LETTER_TOPIC, order.getId(), order)
                        .get(5, TimeUnit.SECONDS); // Wait for send confirmation

                LOGGER.info("Sent order {} to dead letter queue", order.getId());
            } catch (Exception e) {
                throw new RuntimeException("Failed to send to DLQ", e);
            }
        });
    }

    /**
     * Custom exception to indicate non-retryable errors
     */
    public static class NonRetryableException extends RuntimeException {
        public NonRetryableException(String message, Throwable cause) {
            super(message, cause);
        }
    }

    /**
     * Wrapper class for orders sent to DLQ with error information
     */
    public static class OrderWithError {
        private final Order order;
        private final String errorMessage;
        private final long timestamp;

        public OrderWithError(Order order, String errorMessage) {
            this.order = order;
            this.errorMessage = errorMessage;
            this.timestamp = System.currentTimeMillis();
        }

        // Getters omitted for brevity
    }
}