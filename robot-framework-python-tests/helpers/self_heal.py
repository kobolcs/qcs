from robot.api import logger
from robot.libraries.BuiltIn import BuiltIn
from Browser import Browser

def click_with_fallback(selectors):
    b = BuiltIn().get_library_instance("Browser")
    for sel in selectors:
        try:
            b.click(sel)
            logger.info(f"Clicked using selector: {sel}")
            return sel
        except Exception as e:
            logger.warn(f"Selector failed: {sel}: {e}")
    raise Exception("None of the selectors worked")
