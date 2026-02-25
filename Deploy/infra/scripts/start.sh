#!/usr/bin/env bash
set -euo pipefail

# ===== Ajustá esto =====
APP_DIR="/home/ubuntu/deploy"
DLL_NAME="DirtyApi.dll"
PORT="5079"
ENVIRONMENT="Development"
# =======================

echo "=== Starting .NET app ==="
echo "APP_DIR=${APP_DIR}"
echo "DLL=${DLL_NAME}"
echo "PORT=${PORT}"
echo "ENV=${ENVIRONMENT}"

cd "${APP_DIR}"

# Variables típicas
export ASPNETCORE_ENVIRONMENT="${ENVIRONMENT}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

# (Opcional) si tu app lee config externa
# export DOTNET_ENVIRONMENT="${ENVIRONMENT}"

# Mata cualquier proceso anterior que esté ejecutando ese DLL (best-effort)
echo "Stopping previous instance (best-effort)..."
pkill -f "dotnet .*${DLL_NAME}" || true

# Arranque en background, dejando logs en archivo (evita que CodeDeploy quede “colgado”)
LOG_DIR="/var/log/myapi"
mkdir -p "${LOG_DIR}"
nohup dotnet "${DLL_NAME}" > "${LOG_DIR}/app.out.log" 2> "${LOG_DIR}/app.err.log" < /dev/null &

sleep 2

echo "Process check:"
pgrep -af "dotnet .*${DLL_NAME}" || (echo "❌ No se encontró el proceso levantado" && exit 1)

echo "Port check:"
ss -ltnp | grep ":${PORT} " || echo "⚠️ No veo el puerto escuchando aún (puede tardar un poco)"

echo "✅ Start script finished"