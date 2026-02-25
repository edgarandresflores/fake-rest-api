#!/usr/bin/env bash
set -euo pipefail

APP_NAME="MyApi.dll"

echo "Stopping (best-effort)..."
pkill -f "dotnet .*${APP_NAME}" || true
echo "Done."