#!/usr/bin/env bash
set -euo pipefail
KHMOD_INSTALL_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
python3 - "$KHMOD_INSTALL_ROOT" "${1:-}" <<'PY'
from pathlib import Path
from datetime import datetime
import hashlib
import shutil
import subprocess
import sys

root = Path(sys.argv[1])
launch = sys.argv[2] == '--launch'
if sys.argv[2] not in ('', '--launch'):
    sys.exit('Usage: scripts/install-macos.sh [--launch]')
app = Path.home() / 'Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app'
target = app / 'Contents/Resources/Data/Managed/Mods/Gauntlet'
staged = root / 'artifacts/Gauntlet'
names = ('Gauntlet.dll', 'Gauntlet.Core.dll', 'README.md')
if not target.is_dir() or not all((staged / name).is_file() for name in names):
    sys.exit('Existing installation or staged build missing; no files changed.')
process = subprocess.run(['pgrep', '-fl', r'hollow_knight\.app/Contents/MacOS|Hollow Knight\.app/Contents/MacOS'], capture_output=True, text=True)
if process.returncode != 1:
    sys.exit('Close Hollow Knight before installing. Process check: ' + (process.stdout or process.stderr))
backup = root / '.local/backups' / ('before-gauntlet-0.14.0-' + datetime.now().strftime('%Y%m%d-%H%M%S-%f'))
(backup / 'saves').mkdir(parents=True)
for save in (Path.home() / 'Library/Application Support/unity.Team Cherry.Hollow Knight').glob('*.dat'):
    shutil.copy2(save, backup / 'saves' / save.name)
for name in names:
    if (target / name).exists():
        shutil.copy2(target / name, backup / name)
try:
    for name in names:
        shutil.copy2(staged / name, target / name)
        if hashlib.sha256((staged / name).read_bytes()).digest() != hashlib.sha256((target / name).read_bytes()).digest():
            raise RuntimeError('Installed hash mismatch: ' + name)
except Exception:
    for name in names:
        if (backup / name).exists():
            shutil.copy2(backup / name, target / name)
    raise
print('Installed and hash-verified Gauntlet 0.14.0.')
print('Backup: ' + str(backup))
if launch:
    subprocess.run(['open', str(app)], check=True)
PY
