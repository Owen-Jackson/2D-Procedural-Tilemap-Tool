# 2D-Procedural-Tilemap-Tool
This tool is a basic tilemap editor that can be used to create top-down 2D game dungeons. It was made with Unity version 5.6.3f1.

##Features
- Currently the only placeable tiles are room tiles and corridor tiles. Sprites will autotile to neighbouring tiles of the same type.
- To join a room to a corridor you mark the two adjacent tiles you want to link as exits with the associated editor function.
- The editor has a generate button that can be used to procedurally generate entire floor layouts. 
- The clear button will reset the current floor's layout.
- A dungeon can have multiple floors.
  - Use the '+' button to add a new floor to the current dungeon.
  - Use the bin button to delete the current floor.
  - The arrow buttons on the editor menu are used to navigate between floors.
- Dungeons can be saved and loaded from the Save/Load editor tab. There is text input for naming the dungeon to save/load.

**WARNING:** there is no undo functionality, the actions of clearing or deleting floors cannot be reverted.

##Controls
Left Mouse Button: Navigate the editor menu
Left and Right mouse buttons: Paint on the tilemap
WASD: Move the camera
Escape: Quit the application

##Notable Underlying Functionality
The editor uses 4-bit bitmasking to autotile as you edit the level layout. This checks the adjacent tiles to the North, East, South and West of the placed tile and updates all of their sprites accordingly.
When generating floor layouts, the program uses a Binary Space Partition (BSP) Tree structure to create the rooms and then connects the exits with the A* algorithm. Dungeon floors made with this are guaranteed to be completable (i.e. a player can move to anywhere on the generated floor from their starting point).

##Sources
Credit to g4mersylver for the dungeon tileset. Downloaded from OpenGameArt https://opengameart.org/content/dungeon-tileset-rougelike-16x16  Some of the original sprites were edited for the autotiling functionality.

##Useful Resources
[pcgbook](http://pcgbook.com/) was very useful when researching procedural algorithms for level generation. [Chapter 3](http://pcgbook.com/wp-content/uploads/chapter03.pdf) in particular was where I found out about BSP Trees.
[This tutorial](https://gamedevelopment.tutsplus.com/tutorials/how-to-use-tile-bitmasking-to-auto-tile-your-level-layouts--cms-25673) was the main source I used when implementing the bitmask autotiling.


This project was made for a final-year assignment for my Advanced Technologies module.