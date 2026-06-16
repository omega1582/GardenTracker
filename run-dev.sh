#!/bin/bash

# Configuration
export PATH="$HOME/.dotnet:$PATH"
export ASPNETCORE_ENVIRONMENT=Development

# Ensure we are in the script's directory
cd "$(dirname "$0")"

echo "========================================="
echo " Starting GardenTracker Dev Environment"
echo "========================================="

# Start backend
echo "-> Starting Backend API (.NET 10)..."
dotnet run --project GardenTracker.Api --launch-profile http &
BACKEND_PID=$!

# Start frontend
echo "-> Starting Frontend Web (Vite)..."
cd GardenTracker.Web && npm run dev &
FRONTEND_PID=$!

# Handle shutdown gracefully (Ctrl+C)
cleanup() {
    echo ""
    echo "========================================="
    echo " Shutting down dev servers..."
    echo "========================================="
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit
}

trap cleanup INT TERM EXIT

# Keep script running
wait
