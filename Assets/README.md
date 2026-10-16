# Treasure Hunt



A 3D maze game made in Unity for the Game Developer job test. Find the hidden treasure as fast as you can while avoiding patrolling guards and floor traps.





## Time Spent



**5 hours to make the game**



## How to Play



|Action|Control|
|-|-|
|Move|WASD / Arrow keys|
|Look|Mouse|
|Sprint|Left Shift|
|Jump|Space|





* Reach the **glowing treasure sphere**, which is always placed at the farthest point of the maze from your start.
* **Guards** patrol the corridors. Each one shows its **vision cone on the floor**. If you stay inside a cone, they spot you and it's **Game Over**.
* **Spike traps** cycle through hidden, warning (spikes peek out) and active (spikes up). Stepping on active spikes costs 1 health.
* You have **3 health**. After a hit you get 1 second of protection.
* A timer tracks your run, and your **best time** is saved.



## Running the Project



* **Unity version:** Unity 6 (6000.0.51f1)
* **Render pipeline:** URP
* **Packages used:** AI Navigation, Cinemachine, Input System, Starter Assets (Third Person)
1. Clone the repo and open it with Unity Hub.
2. Open the `MainMenu` scene and press Play (both scenes are in the Build Profiles scene list).





## Maze Generator Editor Window



Open it from **Tools > Treasure Hunt > Maze Generator**.



1. Assign a **Maze Settings** asset (or create one with *Create > Treasure Hunt > Maze Settings*).
2. Assign the **Enemy Prefab** and **Trap Prefab** (the Wall Prefab is optional and falls back to plain cubes).
3. Edit the parameters right in the window:

   * **Width / Height / Cell Size**: maze size
   * **Wall Height / Wall Thickness**
   * **Seed**: the same seed always gives the same maze, enemy layout and trap layout
   * **Trap Count / Enemy Count**
4. Click **Generate Maze**. Use **Randomize Seed On Generate** for a new layout each time, or untick it to regenerate the same one.
5. **Clear Maze** removes everything. Generating and clearing both support **Ctrl+Z**.

Each generation builds the walls and floor, places `PlayerStart`, puts the treasure at the farthest cell, and spawns the enemies (with patrol waypoints) and traps. Settings values are validated, so a designer can't enter something that breaks the maze.



## How It Works



* **Maze generation:** a recursive backtracker (randomized DFS, written with a stack so large mazes are safe) produces a maze where every cell is reachable. A BFS from the start finds the farthest cell for the treasure.
* **Data and logic are separate:** `MazeGenerator` is pure logic with no GameObjects, `MazeBuilder` turns the grid into walls and floor, and the `MazeSettings` ScriptableObject holds the designer-editable data.
* **NavMesh:** `MazeNavMeshBaker` bakes the NavMesh from the maze geometry when the game starts, so enemies always match the current layout.
* **Enemies:** `EnemyPatrol` walks between waypoints with a NavMeshAgent. `EnemyVision` handles the field of view: angle check, distance check and a raycast for walls. It draws a wall-clipped cone mesh and gives a short reaction time before Game Over.
* **Game flow:** `GameManager` holds the state and the timer, and exposes events (`OnGameOver`, `OnWin`, `OnEndScreen`). UI and effects listen to those events, so gameplay scripts don't depend on them.
* **UI:** the main menu, HUD, Game Over and Win screens are built from code by `UIFactory`, `MainMenuUI` and `GameUI`.
* **Juice:** camera shake, red hit particles and a screen flash on damage, slow motion before the end screens, golden particles on the treasure, dust on spike traps, and sound effects. The sounds are synthesized in code (`Sfx`), so the project has no audio file dependencies.



## Project Structure

```
Assets/
  Editor/       MazeEditorWindow
  Scripts/
    Maze/       MazeSettings, MazeGenerator, MazeBuilder, MazeNavMeshBaker
    Player/     PlayerHealth, PlayerFeedback
    Enemy/      EnemyPatrol, EnemyVision
    Traps/      SpikeTrap
    Core/       GameManager, TreasureGoal, GameConstants
    UI/         UIFactory, GameUI, MainMenuUI, Sfx, JuiceFx
  Prefabs/      Enemy, SpikeTrap, Wall
  Data/         Maze Settings asset
  Scenes/       MainMenu, Game
```

## Notes and Possible Improvements



* Replace the placeholder visuals (plain cubes, capsule enemy, sphere treasure) with proper models and textures.
* Swap the synthesized sounds for recorded audio and add background music.
* Add keys and locked doors, and moving platforms, using the same cell data the generator already produces.
* Add difficulty levels (several Maze Settings assets), and a minimap.
* Add enemy chase behaviour and hearing, not just sight.
* Build and publish a standalone version (Windows/WebGL).



## Credits

* Character and camera: Unity Starter Assets (Third Person Controller)

