from BrowserLibrary import Browser
from robot.api import logger

class CustomPlaywright:
    def click_shadow_button(self, host_selector, button_selector):
        browser = Browser()._current_browser()
        page = browser._current_page()
        host = page.query_selector(f"css={host_selector}")
        if not host:
            raise AssertionError(f"Host element not found: {host_selector}")
        shadow_btn = host.query_selector(f"css={button_selector}")
        if not shadow_btn:
            raise AssertionError(f"Shadow button not found: {button_selector}")
        shadow_btn.click()
        logger.info(f"Clicked shadow button {button_selector} inside {host_selector}")
