from robot.libraries.BuiltIn import BuiltIn

class DynamicTestCreator:
    """Keyword library to create Robot Framework tests at runtime."""

    ROBOT_LIBRARY_SCOPE = "GLOBAL"

    def create_test(self, name: str, keyword: str, *args):
        """Create a test case dynamically and assign it the given name."""
        BuiltIn().create_test(name, keyword, *args)
