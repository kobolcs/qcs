import pytest
from appium.webdriver.common.appiumby import AppiumBy
from helpers.appium_driver import create_driver

class TestBasicNavigation:
    """A test suite for basic navigation in the ApiDemos app."""

    @pytest.fixture(scope="class")
    def driver(self):
        """Fixture to set up and tear down the Appium driver."""
        driver = create_driver()
        yield driver
        driver.quit()

    def test_navigate_to_views_and_check_title(self, driver):
        """
        Tests navigation to the 'Views' section and verifies the title.
        This demonstrates finding elements by accessibility ID and validating attributes.
        """
        # 1. Find the "Views" element by its accessibility ID and click it.
        # Accessibility IDs are often the most reliable locators on mobile.
        views_element = driver.find_element(by=AppiumBy.ACCESSIBILITY_ID, value="Views")
        assert views_element.is_displayed(), "The 'Views' option should be visible."
        views_element.click()

        # 2. After clicking, verify we are on the new screen by checking for a known element.
        # We find the title element by its ID.
        title_element = driver.find_element(by=AppiumBy.ID, value="android:id/text1")

        # 3. Assert that the title element's text is "Views".
        assert title_element.text == "Views", f"Expected title to be 'Views', but found '{title_element.text}'."

