import time
from functools import wraps
from robot.api import logger

def retry_with_backoff(max_attempts=3, backoff_factor=2, exceptions=(Exception,)):
    """Retry decorator with exponential backoff."""
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            attempt = 1
            delay = 1

            while attempt <= max_attempts:
                try:
                    result = func(*args, **kwargs)
                    if attempt > 1:
                        logger.info(f"Succeeded after {attempt} attempts")
                    return result
                except exceptions as e:
                    if attempt == max_attempts:
                        logger.error(f"Failed after {max_attempts} attempts: {e}")
                        raise
                    logger.warn(f"Attempt {attempt} failed: {e}. Retrying in {delay}s...")
                    time.sleep(delay)
                    delay *= backoff_factor
                    attempt += 1
        return wrapper
    return decorator

# For Robot Framework keywords
class RetryKeywords:
    """Keywords for handling flaky operations."""

    def retry_keyword(self, keyword, *args, **kwargs):
        """Retry any keyword with exponential backoff."""
        from robot.libraries.BuiltIn import BuiltIn
        bi = BuiltIn()

        for attempt in range(3):
            try:
                return bi.run_keyword(keyword, *args, **kwargs)
            except Exception as e:
                if attempt == 2:
                    raise
                time.sleep(2 ** attempt)