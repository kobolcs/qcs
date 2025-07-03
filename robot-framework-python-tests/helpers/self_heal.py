from robot.api import logger
from robot.libraries.BuiltIn import BuiltIn
from Browser import Browser


def click_with_fallback(selectors):
    """Try clicking each selector until one succeeds.

    Args:
        selectors (Iterable[str]): Locator strings to attempt in order.

    Returns:
        str: The selector that resulted in a successful click.

    Raises:
        Exception: If none of the selectors work.
    """
    b = BuiltIn().get_library_instance("Browser")
    for sel in selectors:
        try:
            b.click(sel)
            logger.info(f"Clicked using selector: {sel}")
            return sel
        except Exception as e:
            logger.warning(f"Selector failed: {sel}: {e}")
    raise Exception("None of the selectors worked")
