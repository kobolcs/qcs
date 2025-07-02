package com.eventdriven.order.controller;

import org.springframework.boot.actuate.health.Health;
import org.springframework.boot.actuate.health.HealthIndicator;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.stereotype.Component;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;

@Component
public class CustomHealthIndicator implements HealthIndicator {

    private final JedisPool jedisPool;
    private final KafkaTemplate<String, Object> kafkaTemplate;

    public CustomHealthIndicator(JedisPool jedisPool, KafkaTemplate<String, Object> kafkaTemplate) {
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

            // Check Kafka
            kafkaTemplate.getDefaultTopic();

            return Health.up()
                    .withDetail("redis", "Available")
                    .withDetail("kafka", "Available")
                    .build();

        } catch (Exception e) {
            return Health.down()
                    .withException(e)
                    .build();
        }
    }
}