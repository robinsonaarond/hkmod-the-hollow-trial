#!/usr/bin/env bash
set -euo pipefail
KHMOD_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
case "$(uname -s)" in
  Darwin)
    KHMOD_PLATFORM=macos
    KHMOD_ARCHIVE="$KHMOD_ROOT/.tools/api-macos.zip"
    KHMOD_SHA=6f636af4d831d8892aea984c74d154d558fb23cc0c5b768ab6d7ce4655afac25
    ;;
  Linux)
    KHMOD_PLATFORM=linux
    KHMOD_ARCHIVE="$KHMOD_ROOT/.tools/api-linux.zip"
    KHMOD_SHA=5d2327d76b2c48e2694e43e90689d867eaa9ac1d9e6fd2998c93087de7b61ddb
    ;;
  *) echo 'This setup script supports native macOS and Linux.' >&2; exit 2 ;;
esac
mkdir -p "$KHMOD_ROOT/.tools"
if [[ ! -f "$KHMOD_ARCHIVE" ]]; then
  curl --fail --location --retry 2 \
    "https://github.com/hk-modding/api/releases/download/1.5.78.11833-77/moddingapi.v77.$KHMOD_PLATFORM.zip" \
    -o "$KHMOD_ARCHIVE"
fi
if command -v sha256sum >/dev/null 2>&1; then
  KHMOD_ACTUAL="$(sha256sum "$KHMOD_ARCHIVE" | cut -d ' ' -f 1)"
else
  KHMOD_ACTUAL="$(shasum -a 256 "$KHMOD_ARCHIVE" | cut -d ' ' -f 1)"
fi
if [[ "$KHMOD_ACTUAL" != "$KHMOD_SHA" ]]; then
  echo "API archive checksum mismatch: $KHMOD_ARCHIVE. Remove it and retry." >&2
  exit 1
fi
mkdir -p "$KHMOD_ROOT/.local/api"
unzip -q -o "$KHMOD_ARCHIVE" -d "$KHMOD_ROOT/.local/api"
echo 'Prepared Modding API v77 (game 1.5.78.11833) in .local/api.'
echo 'Only project-local reference files were extracted. The game installation was not changed.'
