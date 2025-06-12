package com.eventdriven.order.kafka;

import com.eventdriven.order.model.Order;
import com.eventdriven.order.redis.OrderStatusRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class OrderEventConsumer {

    private static final Logger LOGGER = LoggerFactory.getLogger(OrderEventConsumer.class);
    private static final String STATUS_PROCESSED = "PROCESSED";
    private static final String DEAD_LETTER_TOPIC = "order_dead_letter_topic";

    private final OrderStatusRepository orderStatusRepository;
    private final KafkaTemplate<String, Order> kafkaTemplate;

    @Autowired
    public OrderEventConsumer(OrderStatusRepository orderStatusRepository, KafkaTemplate<String, Order> kafkaTemplate) {
        this.orderStatusRepository = orderStatusRepository;
        this.kafkaTemplate = kafkaTemplate;
    }

    @KafkaListener(topics = "order_created_topic", groupId = "order_group", containerFactory = "orderKafkaListenerContainerFactory")
    public void handleOrderCreatedEvent(Order order) {
        String orderId = order.getId();
        LOGGER.info("Received order event for ID: {}", orderId);

        try {
            orderStatusRepository.setStatus(orderId, STATUS_PROCESSED);
            LOGGER.info("Successfully processed order and set status to PROCESSED for ID: {}", orderId);
        } catch (Exception e) {
            LOGGER.error("Failed to process order ID: {}, sending to dead-letter topic", orderId, e);
            // Send the failed order to a dead-letter topic for further investigation
            kafkaTemplate.send(DEAD_LETTER_TOPIC, orderId, order);
        }
    }
}