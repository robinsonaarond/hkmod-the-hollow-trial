#!/usr/bin/env bash
set -euo pipefail
KHMOD_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="$KHMOD_ROOT/.tools/dotnet-home"
export NUGET_PACKAGES="$KHMOD_ROOT/.tools/nuget"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
if [[ -x "$KHMOD_ROOT/.tools/dotnet/dotnet" ]]; then
  KHMOD_DOTNET="$KHMOD_ROOT/.tools/dotnet/dotnet"
else
  KHMOD_DOTNET="$(command -v dotnet || true)"
fi
if [[ -z "$KHMOD_DOTNET" ]]; then
  echo 'Install .NET SDK 8 or later, then retry.' >&2
  exit 1
fi
