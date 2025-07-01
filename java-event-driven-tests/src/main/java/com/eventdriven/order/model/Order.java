package com.eventdriven.order.model;

import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.Objects;
import java.util.regex.Pattern;

/**
 * Immutable Order domain model with proper validation and JSON serialization
 * support.
 */
public class Order {

    private static final int MAX_ID_LENGTH = 100;
    private static final int MAX_DESCRIPTION_LENGTH = 1000;
    private static final Pattern VALID_ID_PATTERN = Pattern.compile("^[a-zA-Z0-9-_]+$");

    private final String id;
    private final String description;

    @JsonCreator
    private Order(@JsonProperty("id") String id,
            @JsonProperty("description") String description) {
        this.id = validateId(id);
        this.description = validateDescription(description);
    }

    private Order(Builder builder) {
        this.id = validateId(builder.id);
        this.description = validateDescription(builder.description);
    }

    private static String validateId(String id) {
        Objects.requireNonNull(id, "Order id must not be null");
        String trimmedId = id.trim();

        if (trimmedId.isEmpty()) {
            throw new IllegalArgumentException("Order id must not be empty");
        }

        if (trimmedId.length() > MAX_ID_LENGTH) {
            throw new IllegalArgumentException(
                    String.format("Order id must not exceed %d characters", MAX_ID_LENGTH));
        }

        if (!VALID_ID_PATTERN.matcher(trimmedId).matches()) {
            throw new IllegalArgumentException(
                    "Order id must contain only alphanumeric characters, hyphens, and underscores");
        }

        return trimmedId;
    }

    private static String validateDescription(String description) {
        if (description == null) {
            return null; // Description is optional
        }

        String trimmedDesc = description.trim();

        if (trimmedDesc.length() > MAX_DESCRIPTION_LENGTH) {
            throw new IllegalArgumentException(
                    String.format("Order description must not exceed %d characters", MAX_DESCRIPTION_LENGTH));
        }

        // Sanitize to prevent injection attacks
        return trimmedDesc.replaceAll("[<>\"']", "");
    }

    public String getId() {
        return id;
    }

    public String getDescription() {
        return description;
    }

    public static Builder builder() {
        return new Builder();
    }

    public static class Builder {
        private String id;
        private String description;

        public Builder id(String id) {
            this.id = id;
            return this;
        }

        public Builder description(String description) {
            this.description = description;
            return this;
        }

        public Order build() {
            return new Order(this);
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o)
            return true;
        if (o == null || getClass() != o.getClass())
            return false;

        Order order = (Order) o;
        return Objects.equals(id, order.id) &&
                Objects.equals(description, order.description);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id, description);
    }

    @Override
    public String toString() {
        return "Order{" +
                "id='" + id + '\'' +
                ", description='" + (description != null ? description : "null") + '\'' +
                '}';
    }
}