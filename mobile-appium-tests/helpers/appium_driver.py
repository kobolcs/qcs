from appium import webdriver
from appium.options.android import UiAutomator2Options

def create_driver():
    """
    Creates and returns an Appium WebDriver instance for an Android emulator.

    This function encapsulates the desired capabilities for the test session.
    """
    # URL to the ApiDemos app used for testing.
    # In a real project, this would be the path to your application's APK file.
    app_url = "https://github.com/appium/java-client/raw/master/src/test/resources/apps/ApiDemos-debug.apk"

    # Set the desired capabilities for the Android session using UiAutomator2Options.
    # This tells Appium which device, platform, and app to test.
    options = UiAutomator2Options()
    options.platform_name = "Android"
    options.automation_name = "UiAutomator2"

    # In a local run, you would specify your emulator's name, e.g., 'options.device_name = "Pixel_6_API_33"'
    # For CI, these are often handled by the emulator runner action.

    # The path to the app to be installed and tested.
    options.app = app_url

    # The Appium server URL. This is the default.
    appium_server_url = 'http://localhost:4723'

    # Create and return the WebDriver instance with simple error handling.
    try:
        return webdriver.Remote(appium_server_url, options=options)
    except Exception as exc:
        raise RuntimeError(f"Failed to start Appium session: {exc}") from exc

