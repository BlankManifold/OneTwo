# NewbiesJamMyGame

My first game jam ever!
A simple puzzle game made for [Newbies Game Jam (2022 - 1)](https://itch.io/jam/newbies) with MINIMALISM theme!


## Objective


## Progress Updates:

### 18/04/22:

<img align="right" src="stuffForREADME/update1.gif" width="230">

* Created project repository
* Create base scenes: `Block`, `Grid`, `Main` and `GameUI`
* Created base block mechanics: select block, swap two adjoint blocks 
* Created grid generation: creates the grid as a matrix and select corrisponding color from a random list of `colorId` -> if grid is a NxM matrix (rows and columns), there will be N different color; column with index 0 is fixed, always showned and as N unique color; columns with indexes 1 to M are hidden, in these there are a total of M-1 blocks with a specific color
* Created a `RestartButton`: re-randomize the grid
* Created a `Globals` script containing: `RandomManager` struct, some to be filled `enums`, `ColorPalette` static class
