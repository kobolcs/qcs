package com.eventdriven.order.redis;

import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public class OrderStatusRepository {

    // Make the prefix a public constant to be shared with tests
    public static final String REDIS_KEY_PREFIX = "order_status:";

    private final StringRedisTemplate redisTemplate;

    public OrderStatusRepository(StringRedisTemplate redisTemplate) {
        this.redisTemplate = redisTemplate;
    }

    /**
     * Sets the status for the given order ID in Redis.
     * * @param orderId the order ID, must not be null or empty
     *
     * @param status the status to set, must not be null
     */
    public void setStatus(String orderId, String status) {
        if (orderId == null || orderId.isEmpty()) {
            throw new IllegalArgumentException("orderId must not be null or empty");
        }
        if (status == null) {
            throw new IllegalArgumentException("status must not be null");
        }
        redisTemplate.opsForValue().set(buildKey(orderId), status);
    }

    /**
     * Retrieves the status for the given order ID from Redis.
     * * @param orderId the order ID, must not be null or empty
     *
     * @return an Optional containing the status if present, otherwise empty
     */
    public Optional<String> getStatus(String orderId) {
        if (orderId == null || orderId.isEmpty()) {
            throw new IllegalArgumentException("orderId must not be null or empty");
        }
        return Optional.ofNullable(redisTemplate.opsForValue().get(buildKey(orderId)));
    }

    private String buildKey(String orderId) {
        return REDIS_KEY_PREFIX + orderId;
    }
}