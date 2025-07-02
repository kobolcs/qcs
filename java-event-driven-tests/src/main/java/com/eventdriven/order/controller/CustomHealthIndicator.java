package com.eventdriven.order.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.actuate.health.Health;
import org.springframework.boot.actuate.health.HealthIndicator;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.stereotype.Component;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;

@Component
public class CustomHealthIndicator implements HealthIndicator {

    private final JedisPool jedisPool;
    private final KafkaTemplate<String, Order> kafkaTemplate;

    @Autowired
    public CustomHealthIndicator(JedisPool jedisPool,
            @Autowired(required = false) KafkaTemplate<String, Order> kafkaTemplate) {
        this.jedisPool = jedisPool;
        this.kafkaTemplate = kafkaTemplate;
    }

    @Override
    public Health health() {
        try {
            // Check Redis
            try (Jedis jedis = jedisPool.getResource()) {
                jedis.ping();
            }

            // Check Kafka (if available)
            boolean kafkaAvailable = kafkaTemplate != null && kafkaTemplate.getDefaultTopic() != null;

            return Health.up()
                    .withDetail("redis", "Available")
                    .withDetail("kafka", kafkaAvailable ? "Available" : "Not configured")
                    .build();

        } catch (Exception e) {
            return Health.down()
                    .withException(e)
                    .build();
        }
    }
}