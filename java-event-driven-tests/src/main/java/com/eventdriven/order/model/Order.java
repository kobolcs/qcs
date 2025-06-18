package com.eventdriven.order.model;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.Objects;

public class Order {
    @JsonProperty("id")
    private final String id;

    @JsonProperty("description")
    private final String description;

    private Order(Builder builder) {
        this.id = builder.id;
        this.description = builder.description;
    }

    // Default constructor for Jackson Deserialization
    public Order() {
        this.id = null;
        this.description = null;
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
            if (id == null || id.isEmpty()) {
                throw new IllegalArgumentException("Order id must not be null or empty");
            }
            return new Order(this);
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o)
            return true;
        if (!(o instanceof Order))
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
                ", description='" + description + '\'' +
                '}';
    }
}
