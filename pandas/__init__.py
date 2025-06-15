class _ILoc:
    def __init__(self, data):
        self._data = data
    def __getitem__(self, idx):
        return self._data[idx]

class DataFrame:
    def __init__(self, data):
        self._data = list(data)
    @property
    def iloc(self):
        return _ILoc(self._data)

    def __getitem__(self, key):
        return [row.get(key) for row in self._data]

# Simple CSV loader for minimal functionality
import csv

def read_csv(fp):
    if hasattr(fp, 'read'):
        reader = csv.DictReader(fp)
    else:
        f = open(fp, newline='')
        reader = csv.DictReader(f)
    data = [row for row in reader]
    if not hasattr(fp, 'read'):
        f.close()
    return DataFrame(data)
