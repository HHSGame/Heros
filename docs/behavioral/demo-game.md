# Demo Island - HHS Game Editor Demo

This is a simple demo game that showcases the HHS Game Editor capabilities.

## Running the Demo

```bash
# From the project root
./demo-game/run-demo.sh

# Or manually
dotnet run --project src/HHSGame/HHSGame.csproj -- --config demo-game/data/game.json
```

## Game Features

### Map
- 30x20 grid map with walls and open areas
- Two distinct regions (forest and cave)

### NPCs
- **Village Elder** (E) - Gives quests and lore
- **Healer** (H) - Heals player and gives supplies

### Enemies
- **Wolves** (W) - 3 wolves in the forest area
- **Bandits** (B) - 1 bandit in the cave area

### Quests
- **Clear the Island** - Defeat all wolves and bandits
  - Kill 3 wolves
  - Kill 1 bandit
  - Reward: 100 XP + 50 currency

### Items
- Health Potions, Bandages, Bread scattered on map

## Editor Features Demonstrated

1. **Map Editor** - Visual map editing with tile palette
2. **NPC Placement** - Place NPCs with dialogue IDs
3. **Enemy Placement** - Place enemies with spawn counts
4. **Item Placement** - Place items on the map
5. **Dialogue System** - Multi-node dialogue with conditions and effects
6. **Quest System** - Quest objectives and rewards
7. **Lua Scripting** - Healer uses `game.HealPlayer(50)` via Lua

## File Structure

```
demo-game/
├── data/
│   ├── catalogs/
│   │   ├── weapons.json
│   │   ├── armors.json
│   │   ├── items.json
│   │   ├── classes.json
│   │   └── enemies.json
│   ├── maps/
│   │   └── demo-island.txt
│   └── game.json
├── run-demo.sh
└── README.md
```

## Lua Scripting Example

The Healer NPC demonstrates Lua scripting:

```json
{
  "text": "Yes, please heal me.",
  "nextNodeId": "heal",
  "effects": [
    { "type": "LuaScript", "luaScript": "game.HealPlayer(50)" }
  ]
}
```

This shows how Lua scripts can be integrated into dialogue effects.
