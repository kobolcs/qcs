import importlib.util
import sys
from pathlib import Path

_real_module = Path(__file__).resolve().parent.parent / 'ai-test-observability' / 'process_results.py'
_spec = importlib.util.spec_from_file_location(__name__ + '.process_results', _real_module)
_module = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_module)
sys.modules[__name__ + '.process_results'] = _module
__all__ = ['process_results']
