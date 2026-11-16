#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
if [[ $# -ne 2 ]]; then
  echo 'Usage: scripts/build.sh "/path/to/game/Managed" "/path/to/extracted/API"' >&2
  exit 2
fi
if [[ ! -d "$1" ]]; then
  echo "Game Managed directory not found: $1" >&2
  echo 'Pass the Managed directory from Hollow Knight 1.5.78.11833.' >&2
  exit 2
fi
if [[ ! -d "$2" ]]; then
  echo "Modding API directory not found: $2" >&2
  echo 'Run ./scripts/setup-api.sh to prepare .local/api, then retry.' >&2
  exit 2
fi
KHMOD_GAME="$(cd "$1" && pwd)"
KHMOD_API="$(cd "$2" && pwd)"
"$KHMOD_DOTNET" run --project "$KHMOD_ROOT/tools/ReferenceCheck" --configuration Release -p:UseSharedCompilation=false -- "$KHMOD_GAME" "$KHMOD_API"
"$KHMOD_DOTNET" build "$KHMOD_ROOT/src/Gauntlet.Mod" --configuration Release -p:UseSharedCompilation=false "-p:GameManagedPath=$KHMOD_GAME" "-p:ApiManagedPath=$KHMOD_API"
mkdir -p "$KHMOD_ROOT/artifacts/Gauntlet"
cp "$KHMOD_ROOT/src/Gauntlet.Mod/bin/Release/net472/Gauntlet.dll" "$KHMOD_ROOT/artifacts/Gauntlet/"
cp "$KHMOD_ROOT/src/Gauntlet.Core/bin/Release/netstandard2.0/Gauntlet.Core.dll" "$KHMOD_ROOT/artifacts/Gauntlet/"
cp "$KHMOD_ROOT/README.md" "$KHMOD_ROOT/artifacts/Gauntlet/"
echo "Built $KHMOD_ROOT/artifacts/Gauntlet. Nothing has been installed into the game."
