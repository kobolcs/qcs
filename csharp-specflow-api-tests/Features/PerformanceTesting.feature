Feature: API Performance Testing
  As a performance-focused QA engineer
  I want to validate API performance characteristics
  To ensure the system meets performance requirements and SLAs

  Background:
    Given the performance testing environment is configured
    And baseline performance metrics are established
    And monitoring tools are active

  @performance @response_time @smoke
  Scenario: Response time validation for core operations
    Given I am measuring API response times
    When I perform standard booking operations
    Then response times should be within acceptable limits
    And 95th percentile response time should be under 2000ms
    And average response time should be under 500ms
    And no operation should exceed 5000ms timeout

  @performance @load_testing @stress
  Scenario: Load testing under normal conditions
    Given I am conducting load testing
    When I simulate 10 concurrent users for 60 seconds
    Then the system should maintain stable performance
    And error rate should remain below 1%
    And response times should not degrade significantly
    And throughput should meet expected levels

  @performance @stress_testing @peak_load
  Scenario: Stress testing under peak conditions
    Given I am conducting stress testing
    When I simulate 50 concurrent users for 120 seconds
    Then the system should handle the increased load gracefully
    And critical functionality should remain available
    And response times may increase but should not exceed thresholds
    And the system should recover quickly when load decreases

  @performance @spike_testing @burst_load
  Scenario: Spike testing with sudden load increases
    Given I am conducting spike testing
    When I suddenly increase load from 5 to 30 concurrent users
    Then the system should adapt to the load spike
    And performance should stabilize within 30 seconds
    And no requests should fail due to the spike
    And system resources should be efficiently utilized

  @performance @volume_testing @data_load
  Scenario: Volume testing with large datasets
    Given I am testing with large data volumes
    When I create and retrieve bookings with large payloads
    Then response times should scale appropriately with data size
    And memory usage should remain within acceptable limits
    And database performance should not degrade significantly
    And pagination should work efficiently for large result sets

  @performance @endurance_testing @sustained_load
  Scenario: Endurance testing for sustained operations
    Given I am conducting endurance testing
    When I maintain moderate load for an extended period
    Then system performance should remain stable over time
    And no memory leaks should be detected
    And response times should not gradually increase
    And system resources should be properly managed

  @performance @concurrency @parallel_operations
  Scenario: Concurrent operation performance
    Given I am testing concurrent operations
    When multiple users perform booking operations simultaneously
    Then operations should complete without interference
    And data consistency should be maintained
    And deadlocks should not occur
    And concurrent performance should be acceptable

  @performance @throughput @capacity
  Scenario Outline: Throughput testing for different operation types
    Given I am measuring throughput for <operation_type>
    When I perform <request_count> <operation_type> requests
    Then throughput should meet the expected rate
    And requests per second should be within acceptable range
    And resource utilization should be efficient
    And bottlenecks should be identified if present

    Examples:
      | operation_type | request_count |
      | create_booking | 100          |
      | get_booking    | 200          |
      | update_booking | 50           |
      | delete_booking | 50           |

  @performance @resource_utilization @monitoring
  Scenario: Resource utilization monitoring
    Given I am monitoring system resources during testing
    When I perform various API operations under load
    Then CPU utilization should remain within acceptable limits
    And memory usage should be stable and efficient
    And network bandwidth should be optimally used
    And database connections should be properly managed

  @performance @scalability @horizontal_scaling
  Scenario: Horizontal scalability validation
    Given I am testing system scalability
    When load is increased beyond single instance capacity
    Then the system should scale horizontally if configured
    And load distribution should be balanced
    And performance should improve with additional instances
    And scaling should be transparent to users

  @performance @caching @optimization
  Scenario: Caching and optimization effectiveness
    Given I am testing caching mechanisms
    When I repeatedly request the same data
    Then subsequent requests should be faster due to caching
    And cache hit rates should be optimal
    And cache invalidation should work correctly
    And overall system performance should benefit from caching

  @performance @database @query_performance
  Scenario: Database query performance
    Given I am testing database query performance
    When I perform data-intensive operations
    Then database queries should execute efficiently
    And query execution times should be optimized
    And database indexes should be utilized effectively
    And complex queries should not cause performance issues

  @performance @network @latency_testing
  Scenario: Network latency impact assessment
    Given I am testing network latency impact
    When I simulate various network conditions
    Then the API should handle network latency gracefully
    And timeouts should be appropriately configured
    And retry mechanisms should function correctly
    And user experience should remain acceptable

  @performance @memory @memory_usage
  Scenario: Memory usage and garbage collection
    Given I am monitoring memory usage patterns
    When I perform sustained API operations
    Then memory usage should remain stable
    And garbage collection should not impact performance significantly
    And memory leaks should not be present
    And memory allocation should be efficient

  @performance @baseline @regression
  Scenario: Performance regression testing
    Given I have established performance baselines
    When I test the current system version
    Then performance should not have regressed from baseline
    And improvements should be measurable where expected
    And any performance changes should be documented
    And regression thresholds should not be exceeded

  @performance @monitoring @alerting
  Scenario: Performance monitoring and alerting
    Given performance monitoring is configured
    When system performance deviates from normal patterns
    Then appropriate alerts should be triggered
    And performance metrics should be accurately recorded
    And monitoring dashboards should reflect current state
    And historical performance data should be maintained

  @performance @recovery @resilience
  Scenario: Performance during failure recovery
    Given I am testing performance during recovery scenarios
    When the system recovers from simulated failures
    Then performance should return to normal levels quickly
    And recovery time should meet requirements
    And no performance issues should persist after recovery
    And system stability should be maintained

  @performance @api_limits @rate_limiting
  Scenario: Rate limiting performance impact
    Given rate limiting is configured
    When I approach rate limiting thresholds
    Then performance should remain stable within limits
    And rate limiting should not impact legitimate users
    And rate limit responses should be fast
    And rate limiting should effectively prevent abuse

  @performance @data_transfer @payload_optimization
  Scenario: Data transfer and payload optimization
    Given I am testing data transfer efficiency
    When I send requests with various payload sizes
    Then data transfer should be optimized
    And compression should be utilized where appropriate
    And large payloads should not cause timeouts
    And bandwidth usage should be efficient

  @performance @real_world @user_simulation
  Scenario: Real-world usage pattern simulation
    Given I am simulating real-world usage patterns
    When I execute realistic user workflows
    Then performance should meet real-world expectations
    And user experience should be optimal
    And business operations should not be impacted
    And SLA requirements should be consistently met