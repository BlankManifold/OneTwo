# NewbiesJamMyGame

My first game jam ever!
A simple puzzle game made for [Newbies Game Jam (2022 - 1)](https://itch.io/jam/newbies) with MINIMALISM theme!


## Objective


## Progress Updates:

### 18/04/22:

<img align="right" src="stuffForREADME/update1.gif" width="215">

* Created project repository
* Create base scenes: `Block`, `Grid`, `Main` and `GameUI`
* Created base block mechanics: select block, swap two adjoint blocks 
* Created grid generation: creates the grid as a matrix and select corrisponding color from a random list of `colorId` -> if grid is a NxM matrix (rows and columns), there will be N different color; column with index 0 is fixed, always showned and as N unique color; columns with indexes 1 to M are hidden, in these there are a total of M-1 blocks with a specific color
* Created a `RestartButton`: re-randomize the grid
* Created a `Globals` script containing: `RandomManager` struct, some to be filled `enums`, `ColorPalette` static class


### 19/04/22:
<img align="right" src="stuffForREADME/update2.gif" width="215">

The code is already a mess... `Grid.cs` is the most chaotic scripts ever... too many edge-cases, too many async/await, state machine not utilized enough, too many if, too many row of code in a single file... as now, I'm good with this delirious mess... it works... I will refactor in the future, maybe post jam! 

* Completed base game-mechanics (at least a solid base), the mess in `Grid.cs` I talked about:  movements, win-condition, edge-cases, valid-movement-condition, collapsing, check if same color
* Created connection between `Grid` and `GameUI` to update moves counter
* Added `GRIDSTATE`: basically IDLE versus `Tween`/`Timer` activating
* Some game balancing: supermoves (click one and then swap after one color is shown) count 2 moves