# TODO

## Gameplay Enhancements

### Implement more enemy types and behaviors

-   [ ] Design new enemy types with unique stats, abilities, and attack patterns.
    -   [ ] Brainstorm at least 3 new enemy types (e.g., Goblin, Orc, Mage).
    -   [ ] Define stats for each new enemy type (Health, Strength, Defense, Speed).
    -   [ ] Design unique abilities and attack patterns for each enemy type.
        -   [ ] Goblin: Weak melee attacker, can move quickly.
        -   [ ] Orc: Strong melee attacker, slow movement.
        -   [ ] Mage: Ranged attacker, casts spells with various effects.
    -   [ ] Implement configuration for each enemy type in `EnemyRegistry.cs`.
-   [ ] Implement AI for new enemy types.
    -   [ ] Implement basic AI for each enemy type (e.g., chase player, attack player).
    -   [ ] Implement more advanced AI (e.g., patrol, flee, use abilities strategically).

### Implement more item types and effects

-   [ ] Design new item types with various effects (e.g., stat boosts, healing, temporary abilities).
    -   [ ] Brainstorm at least 3 new item types (e.g., Mana Potion, Strength Elixir, Scroll of Teleportation).
    -   [ ] Define effects for each new item type.
        -   [ ] Mana Potion: Restores mana points.
        -   [ ] Strength Elixir: Temporarily increases strength.
        -   [ ] Scroll of Teleportation: Teleports the player to a random location on the map.
    -   [ ] Implement configuration for each item type in `ItemFactory.cs`.
-   [ ] Implement item usage logic.
    -   [ ] Implement the `Use` method for each new item type.
    -   [ ] Update the `UtilityWindow` to allow the player to use the new items.

### Implement a combat system

-   [ ] Implement different attack types (e.g., melee, ranged, magic).
    -   [ ] Define different attack types with varying damage, range, and accuracy.
    -   [ ] Implement attack type selection in the UI.
-   [ ] Implement defense mechanics (e.g., blocking, dodging, armor).
    -   [ ] Implement blocking and dodging mechanics.
    -   [ ] Implement armor that reduces damage taken.
-   [ ] Implement special abilities and skills for players and enemies.
    -   [ ] Design special abilities and skills for players and enemies.
    -   [ ] Implement ability/skill activation logic.
    -   [ ] Implement cooldowns for abilities/skills.

### Implement a leveling system

-   [ ] Implement experience point (XP) gain from defeating enemies and completing quests.
    -   [ ] Calculate XP based on enemy difficulty.
    -   [ ] Store player's XP.
-   [ ] Implement level-up logic with stat increases and new abilities.
    -   [ ] Define level requirements.
    -   [ ] Implement stat increase logic on level up.
    -   [ ] Implement ability/skill unlocking on level up.

### Add a story and quests

-   [ ] Develop a compelling storyline.
    -   [ ] Brainstorm a basic storyline.
    -   [ ] Write dialogue for key characters.
-   [ ] Design and implement quests with objectives, rewards, and dialogue.
    -   [ ] Design at least 3 quests with varying objectives (e.g., kill a certain number of enemies, retrieve an item, explore a location).
    -   [ ] Implement quest tracking and completion logic.
    -   [ ] Implement quest rewards (e.g., XP, items, gold).

## UI Improvements

### Improve the UI

-   [ ] Add more information to the UI, such as the player's health, mana, and experience.
    -   [ ] Display player's health in the UI.
    -   [ ] Display player's mana in the UI (if mana is implemented).
    -   [ ] Display player's experience in the UI.
-   [ ] Make the UI more visually appealing.
    -   [ ] Experiment with different color schemes and layouts.
    -   [ ] Add visual effects (e.g., animations, transitions).
-   [ ] Implement a better inventory management system.
    -   [ ] Allow the player to sort and filter their inventory.
    -   [ ] Implement item tooltips with detailed information.

## Code Improvements

### Refactor the code

-   [ ] Identify areas of the code that are unclear or that could be improved.
    -   [ ] Review the code for potential areas of improvement.
    -   [ ] Make a list of specific code sections to refactor.
-   [ ] Refactor the code to make it more readable, maintainable, and efficient.
    -   [ ] Apply design patterns where appropriate.
    -   [ ] Reduce code duplication.
    -   [ ] Improve code performance.
-   [ ] Add comments and documentation to the code.
    -   [ ] Add comments to explain complex logic.
    -   [ ] Generate API documentation.

## Other

-   [ ] Experiment with the game
    -   [ ] Try different things in the game to see how they work.
    -   [ ] Identify any bugs or issues.
-   [ ] Ask questions
    -   [ ] If you have any questions about the code, ask the developer for clarification.
