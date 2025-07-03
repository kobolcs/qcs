# helpers/contract_validator.py
import json
import jsonschema
from robot.api import logger

class ContractValidator:
    """Validates API responses against contracts."""

    def validate_json_schema(self, json_data: dict, schema_file: str):
        """Validate JSON data against a schema file."""
        with open(schema_file, 'r') as f:
            schema = json.load(f)

        try:
            jsonschema.validate(json_data, schema)
            logger.info(f"JSON validated successfully against {schema_file}")
        except jsonschema.ValidationError as e:
            raise AssertionError(f"Schema validation failed: {e.message}")

    def validate_api_contract(self, response: dict, contract_name: str):
        """Validate API response against predefined contract."""
        contract_path = f"contracts/{contract_name}.json"
        self.validate_json_schema(response, contract_path)