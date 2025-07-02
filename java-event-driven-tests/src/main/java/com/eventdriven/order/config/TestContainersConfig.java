package com.eventdriven.order.config;

import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.Bean;
import org.testcontainers.containers.KafkaContainer;
import org.testcontainers.containers.GenericContainer;
import org.testcontainers.utility.DockerImageName;

@TestConfiguration
public class TestContainersConfig {

    @Bean
    public KafkaContainer kafkaContainer() {
        KafkaContainer kafka = new KafkaContainer(
                DockerImageName.parse("apache/kafka:3.8.1"))
                .withReuse(true);
        kafka.start();
        return kafka;
    }

    @Bean
    public GenericContainer<?> redisContainer() {
        GenericContainer<?> redis = new GenericContainer<>(
                DockerImageName.parse("redis:7-alpine"))
                .withExposedPorts(6379)
                .withReuse(true);
        redis.start();
        return redis;
    }
}