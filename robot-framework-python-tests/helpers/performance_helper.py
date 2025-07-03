import math
from statistics import mean
from typing import Sequence
from robot.api import logger


class PerformanceHelper:
    """Keywords for calculating and reporting performance metrics."""

    ROBOT_LIBRARY_SCOPE = "GLOBAL"

    def calculate_percentile(self, data: Sequence[float], percentile: float) -> float:
        """Return the value at the given percentile from ``data``.

        Args:
            data: Sequence of numeric values.
            percentile: Desired percentile between 0 and 100.

        Returns:
            float: Calculated percentile value or ``0.0`` when ``data`` is empty.
        """
        if not data:
            return 0.0
        values = sorted(float(x) for x in data)
        k = (len(values) - 1) * (percentile / 100.0)
        f = math.floor(k)
        c = math.ceil(k)
        if f == c:
            return values[int(k)]
        d0 = values[int(f)] * (c - k)
        d1 = values[int(c)] * (k - f)
        return d0 + d1

    def log_performance_report(self, results: Sequence[float], p95: float, p99: float) -> None:
        """Log a formatted performance report to the Robot Framework log."""
        if results:
            numbers = [float(x) for x in results]
            avg = mean(numbers)
            minimum = min(numbers)
            maximum = max(numbers)
        else:
            avg = minimum = maximum = 0.0
            numbers = []

        message = (
            "Performance Report:\n"
            f"Iterations: {len(numbers)}\n"
            f"Average: {avg:.2f} ms\n"
            f"Min: {minimum:.2f} ms\n"
            f"Max: {maximum:.2f} ms\n"
            f"P95: {p95:.2f} ms\n"
            f"P99: {p99:.2f} ms"
        )
        logger.info(message, also_console=True)
