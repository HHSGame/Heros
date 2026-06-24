#!/bin/bash
# Run the demo game with HHS engine

cd "$(dirname "$0")/.."

echo "=== HHS Game Demo ==="
echo "Starting Demo Island..."
echo ""

dotnet run --project src/HHSGame/HHSGame.csproj -- --config demo-game/data/game.json
