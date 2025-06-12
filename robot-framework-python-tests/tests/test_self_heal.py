import pytest
from unittest.mock import patch, MagicMock
from helpers.self_heal import click_with_fallback

# A mock class that simulates the Browser library's behavior
class MockBrowser:
    def __init__(self):
        self.selectors_clicked = []

    def click(self, sel):
        if "notfound" in sel:
            raise Exception("Element not found")
        self.selectors_clicked.append(sel)
        return

@patch('robot.libraries.BuiltIn.BuiltIn.get_library_instance')
def test_click_with_fallback_success(mock_get_instance):
    """Should succeed on the second selector after the first one fails."""
    # Arrange
    mock_browser = MockBrowser()
    mock_get_instance.return_value = mock_browser

    # Act
    sel = click_with_fallback(["#notfound", "#found"])

    # Assert
    assert sel == "#found"
    assert mock_browser.selectors_clicked == ["#found"]

@patch('robot.libraries.BuiltIn.BuiltIn.get_library_instance')
def test_click_with_fallback_fail(mock_get_instance):
    """Should raise an exception if all selectors fail."""
    # Arrange
    mock_browser = MockBrowser()
    mock_get_instance.return_value = mock_browser

    # Act & Assert
    with pytest.raises(Exception, match="None of the selectors worked"):
        click_with_fallback(["#notfound1", "#notfound2"])