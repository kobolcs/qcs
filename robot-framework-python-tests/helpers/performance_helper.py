"""Utility keywords for Robot Framework performance testing."""

from typing import Sequence

from robot.api import logger


class PerformanceHelper:
    """Keywords for calculating and reporting performance metrics."""

    ROBOT_LIBRARY_SCOPE = "GLOBAL"

    def calculate_percentile(self, data: Sequence[float], percentile: float) -> float:
        """Return an approximate percentile from ``data``.

        The values are sorted and the element at the index corresponding to the
        requested percentile is returned. ``0.0`` is returned when ``data`` is
        empty.
        """
        if not 0 <= percentile <= 100:
            raise ValueError("Percentile must be between 0 and 100")
        if not data:
            return 0.0

        sorted_values = sorted(float(x) for x in data)
        index = int(round(percentile / 100.0 * (len(sorted_values) - 1)))
        return sorted_values[index]

    def log_performance_report(
        self, results: Sequence[float], p95: float, p99: float
    ) -> None:
        """Log P95 and P99 latencies to the Robot Framework log."""
        numbers = sorted(float(x) for x in results)
        p95_value = self.calculate_percentile(numbers, 95)
        p99_value = self.calculate_percentile(numbers, 99)
        logger.info(f"P95 latency: {p95_value:.2f} ms", also_console=True)
        logger.info(f"P99 latency: {p99_value:.2f} ms", also_console=True)
