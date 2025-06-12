import pandas as pd
import pytest
from unittest.mock import patch, mock_open
from ai_test_observability.process_results import process_k6_results, load_data

def test_process_k6_results_success():
    """
    Test that k6 results are processed correctly into a DataFrame.
    """
    mock_data = {
        "metrics": {
            "http_req_duration": {
                "values": {
                    "avg": 100,
                    "p(95)": 200
                }
            },
            "http_req_failed": {
                "values": {
                    "rate": 0.1
                }
            }
        }
    }
    df = process_k6_results(mock_data)
    
    # Use standard pytest 'assert' statements
    assert isinstance(df, pd.DataFrame)
    assert df.iloc[0]['avg_duration'] == 100
    assert df.iloc[0]['p95_duration'] == 200
    assert df.iloc[0]['fail_rate'] == 0.1

def test_load_data_file_not_found():
    """
    Test that load_data handles a FileNotFoundError gracefully using pytest.raises.
    """
    with pytest.raises(FileNotFoundError):
        load_data("non_existent_file.json")

@patch("builtins.open", new_callable=mock_open, read_data='{"metrics": {}}')
def test_load_data_success(mock_file):
    """
    Test successful data loading from a JSON file.
    """
    data = load_data("dummy_path.json")
    assert "metrics" in data