#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
"$KHMOD_DOTNET" run --project "$KHMOD_ROOT/tests/Gauntlet.Tests" --configuration Release -p:UseSharedCompilation=false
"$KHMOD_DOTNET" run --project "$KHMOD_ROOT/tests/Gauntlet.Loadout.Tests" --configuration Release -p:UseSharedCompilation=false
"$KHMOD_DOTNET" run --project "$KHMOD_ROOT/tests/Gauntlet.Encounter.Tests" --configuration Release -p:UseSharedCompilation=false
