# ASCII Tilemap Editor

## User journey

Speficy map size (width / height)
-> Create empty map
-> Draw map
-> export map

Specify map size and room count
-> Create random map
-> Adjust map details
-> export map

Load map from files
-> Adjust map details
-> export map

## User Stories

As a game designer, I want to create an ASCII map using this editor by specifiying it's size (width and height), so that I can drawing map tiles using tools.

As a game designer, I want to randomly generate an ASCII map uing this editor, so that I can start from existing map and rooms instead of completely drawing from scratch.

As a game designer, I want to pant paths on the map so that players can navigate through it.

As a game designer, I want to paint using different character sets for different tiles, so that the map looks more detailed and challenging.

As a game designer, I want to draw rooms using box characters, so that the rooms are clearly defined.

As a game designer, I want to fill the map blocks with specific characters so I can create tiles in batch fastly.

As a game designer, I want to add doors, stairs etc so that I can add interactivity to my map.

As a game designer, I want to move the map around the viewport so that I can create large maps without having to scroll.

As a game designer, I want to have a set of groups of characters so that I can pick different characters based on requirement.

As a game designer, I want to export and load my map so that I can save my progress.

As a game designer, I want to zoom in and out my map using buttons and mouse scroll so that I can easily control the scale.

As a game designer, I want to see the coordinates of the cursor on the map so that I can place objects accurately.

As a game designer, I want to have rulers on the edges of the viewport so that I can measure distances.

As a game designer, I want to generate random maps based on the style settings (town, cave, forest), so that I can create different game experience.

## Specific requirement

Map size limit:

- Width: 20 - 65536
- Height: 20 - 65536
- Rooms: 2 - 65536

Specific map tiles:

- ' ' (space character): void space that cannot be passed, meaning walls or invisible walls.
- '.': Passible Floors
- '┌┐└┘─│': Box drawing characters that represents rectangle rooms
- '+': Doors to rooms
- '╱╲': together with box drawing characters to draw irregular shaped caves
- '><': Stairs up/down
- '#': Other kind of obstacles that cannot be passed throught
- '@': Player character
- 'A-Za-z': NPCs, Monsters, etc.
- '*': Lootable items
- '$': Gold / Cash / Currency
