#test
# BattleshipsAda
GitHub Repository for AdaShip - submission for Advanced Programming Module Week 2 

## Challenge Outline
### Analysis
Problem: Use a structured, object-oriented design approach for the concept, design, development, 
testing and potential post completion and further improvements of AdaShip 
(a computer console version/adaptation of the classic paper-based Battleships game).

#### AdaShip
_The below is sourced from the assignment brief. Minor grammatical edits applied. Emphasis mine:_

AdaShip is a clone of the classic ‘Battleship’ game – as a default, AdaShip is a two-player, turn
based game of sea warfare. You and an opponent each place a number of ships on your own board, and you
then alternate turns "firing" torpedoes at each other’s ships. The game is won when one player has
destroyed/sunk all of the other player’s ships.

#### Setup:
By default, each **player** uses two 10x10 **boards** (a shipboard and a targetboard). Ship boards are used 
to initially position and hold a record of the location of your **ships** and any hits made by opponents. The target
board is used to keep track of where you have fired (i.e. guessed) and the outcome; for example – hit or
miss. Board locations are referenced using a single notation type **coordinate** system (‘A1’, ‘F4’, etc). The
coordinate system must be designed to use alphabetic letters (A, B, ...) to represent columns and numbers
(1, 2, ...) to represent rows. For example, a player’s Carrier may be located at B2, B3, B4, B5
and B6.

When placing ships, each player must place all their ships on their shipboard; ships have different sizes and
can be either placed horizontally or vertically; a player’s ships cannot be placed such that any of their ships
are either partly or entirely outside their board’s boundaries, across one (or occupy the same space as) a
previously positioned ship.

By default, there are five types of ships:
1. Carrier - Length 5
2. Battleship - Length 4
3. Destroyer - Length 3
4. Submarine - Length 3
5. Patrol Boat - Length 2

#### Typical gameplay
In a typical two-player game, each player would set up their own shipboard. The default opponent, the 
computer, should decide where to place its ships automatically.
The game is played in turns, where each player ‘fires a torpedo’ (by calling out a board coordinate) and the
opponent indicates whether the ‘torpedo’ resulted in a "hit" or "miss". A "hit" indicates that the called
position corresponded to a valid ship coordinate, otherwise it is a "miss". Players record their called
positions using their targetboard; a record of both “hits” and “misses” should be recorded.

#### Winning
Turns are repeated until all of your opponent’s ships have been ‘sunk/destroyed’; players must indicate as
part of the response to each hit whether an entire ship has been sunk or just a single hit; for example, if our
Carrier is located at B2, B3, B4, B5 and B6 and each position has been hit we need to indicate that the
opponent sunk our ship.

### Initial Design
The emphasis above outlines ideas for core objects in the program. Refinement provides an initial plan for 
class/object names in OO-style:
- A player can be either a 'Human' or 'Computer'
- Each player has a collection of 'Ships'
- In real life, a collection of ships is called a 'Fleet' and is spearheaded by an 'Admiral'
- A 'Board' contains 'Tiles' at specific coordinates that hold a 'Section' of a ship

This leads to a top-level structure as follows:

```
(Abstract class)
Admiral:
  - Board (Ship Board)
    -- Tiles
      --- Coords
      --- Section
  - Board (Targetboard)
    -- Tiles
      --- Coords
      --- Section
  - Fleet
    -- Ships
      --- Section
```
Where `Computer` and `Human` are concrete classes of Admiral. 
Additional properties of each class will be necessary for gameplay such as a `Damage` property for ships, 
hit/miss values for tiles, the ship's type and length, a `Team` property for distinguishing `Fleet`/`Board` objects, 
etc. 

The initial class structure with additionally defined properties is illustrated below:

![Initial Design](https://user-images.githubusercontent.com/74011247/139963426-f2475acb-68a5-4758-94dd-2159b263160e.png)

### Approach
Decomposition of the problem into sub-tasks - each with their own GitHub issue - is the approach I'll be taking.
Along with splitting work on each object/class defined above into its own separate task, the following, taken from
the Challenge Outline, will be used during development:
- By default, each player uses two 10x10 boards
- The coordinate system must be designed to use alphabetic letters (A, B, ...) to represent columns and numbers
(1, 2, ...) to represent rows
- Each player must place all their ships on their shipboard
- Ships have different sizes and can be either placed horizontally or vertically
- A player’s ships cannot be placed such that any of their ships are either partly or entirely outside 
their board’s boundaries, across one (or occupy the same space as) a previously positioned ship
- The computer should decide where to place its ships automatically
- Players record their called positions using their targetboard; a record of both “hits” and “misses” 
should be recorded

Other identified tasks have been taken from the assignment brief and are as follows...

A player must be able to:
-  Select from a game menu to a start a:
   ```
   o One player v computer game
   o Quit
   ```
- Select and place any non-placed or placed ships (as provided via the config file) at any valid position 
on their shipboard
- Clearly indicate the current non-placed and placed ships
- See their current shipboard
- Be robust enough to prevent any invalid behaviours, prevent or correct any illegal placements and avoid 
system issues or errors related to user input
- Allow any non-placed ships to be ‘auto-placed’
- Auto-place all ships
- Support a ‘quit’ game and ‘reset’ shipboard option
- Support a ‘continue’ option if all ships have been placed and the user has confirmed they are happy with 
the current placements

## Development
### Standards
Close attention has been paid to the structure of the program throughout development. Classes and objects strictly
conform to advanced OOP principles, duplicate code is minimised and functions are pure. Design patterns, such
as the singleton pattern, are used to prevent unintended behaviour and interfaces used for grouping similar
behaviour.  

Additionally, I've consciously opted for consistent coding styles to make my code neater and more readable. 
The coding style I've adopted includes the following and more: 
- Private variables are prefixed with an underscore: `_variable`
- Variables are in camel case `variableName`
- Properties are in pascal case `Property { get; set; }`
- Functions are in pascal case `public void FunctionName() { }`
- Constants are in UPPERCASE `const string CONSTANT`
- Implicit type `var` for all variables (Note: this is still _strongly typed_). C# infers the type from the 
right-side assignment
- Modifier and class nesting is appropriate for access level  
- Static access for non-instance-requiring methods/classes 
- Class properties, variables and constants are defined at the top of its class, immediately before the
constructor (if non-static)
- Integrity checks and conditionals are inverted (To prevent nesting / cognitive overhead)

#### Note:
_Indentation and alignment of comments or `=` in variable assignment may appear inconsistent if not using 
the **IntellijJ Rider** IDE_ 

### Commits
Commit #1 (ab144f5)

Changes: 
- Skeletal structure for Ship, Board, Fleet, Admiral, Tile
- Addition of Team enum:
    - TEAM_A or TEAM_B - used to associate Fleet and Board objects
- Addition of Orientation enum:
    - Horizontal, Vertical or None - for Ship placement
- Addition of TileState enum
    - None, Hit, Miss - for tracking tile state during gameplay
---
Commit #2 (cade4f3)

Changes:
- Implemented Board.cs
    - Tile reservation and freeing methods added
    - Utility methods like `GetTileAt(coords)` and `GetTilesBetween(start, end)` added
- Implemented Ship.cs
    - Provided appropriate properties to class such as location, length, sections and orientation
- Added ShipType
    - Used in-tandem with config. Each valid entry in the .ini file is designated a 'ShipType' definition
    - Ship object makes use of a defined ShipType to assign its length/name properties 
- Added Admiral.cs
    - Instantiates Fleet and 2x Board
- Added Fleet.cs
    - Utility methods for ship management contained here 
- Added Config.cs
    - Added config ini file
    - Added Configuration class for returning bundled data as a single object
- Added Controller.cs
    - Made use of singleton pattern for instantiation
- Added PlayerType.cs
    - Human / Computer - used for conditional Admiral instantiation (Replaced by interface in future)
- Removed Team.cs
    - Composition (OOP) makes this redundant
- Amended Enum orderings
    - Moved default/null values to top of enum
---
Commit #3 (ceb88d3)

Changes:
- Admiral now an interface
    - Cleaner and more conformant to OO style
    - Reduces code and conditionals with generic method calling
- Added utility class for I/O
    - Inputs, choice-selection and multi-value outputs made globally available to reduce code duplication
      and improve reliability
- ShipType renamed to ShipInfo for clarity
- Event and method delegate used for updating ship damage
    - Optimises ship destroyed check by only running when damage sustained
    - Removes need to edit modifier access for `Fleet.UpdateDestroyed()` or `Fleet.DestroyedShips`
- Board.cs changes
    - Basic rendering functionality introduced
    - X-axis generation method calculates letter-based column coordinates
    - Utility methods to populate tile array and getting of the next coordinate for a tile's constructor added
    - Coords changed from a string to a `Tuple<int, int>` (x, y) to make internal processing easier
    - Improvements to null checks, method redundancy
    - Code refactoring
    - Fixed issue where private variable `_size` would always default to 10x10
- Package and namespace improvements
- Ship.Section 'Damaged' property added
---
Commit #4 (6e970ac)

Changes:
- IAdmiral (and implementations)
    - Changed name of method `GetTileToAttack()` to `GetTileAsInput()` for more generic usages
    - Implemented initial `AutoPlaceShips()` and `GetTileAsInput()` methods
    - Added 'unplace ship' mode
- Board.cs
    - Added `_boardName` variable to title board during output to prevent confusion
    - Significantly optimised and renamed `ReserveTiles()` to `FindContinuousTilesAt()`
    - Refactored method structure to make navigation of class easier by bundling private methods
      and moving them to the top of the class 
    - Moved coordinate-related methods to new Coordinates.cs utility class
- Added Coordinates.cs
    - Provides utility functions and constants related to coordinates
    - Static, never instantiated
    - Cleans up Board.cs
- Added ability to reload config (Controller.cs)
- Changed Enum value naming from UPPERCASE to PascalCase (Java default to C# default)
- Fixed bug where choice-selection utility function would allow a user to select a negative value
- Misc
    - Added comments
    - Made first letter of Ship name uppercase
    - Improved or refactored null checking
---
Commit #5 (c251ee9)

Changes:
- Larger board support/fixes and formatting
- Implemented basic AI
- Implemented all Human functionality
- Added gameplay logic
- Added GetLetterFromValue() in Coordinates.cs
- Simplified IAdmiral.cs
- Minor console-display edits
---
Commit #6 (ca7c7d8)

Changes:
- Made Fleet a public readonly property of IAdmiral.cs
- Refactored turn-taking
- Added initial support for salvo and mine gamemodes
- Made Destroyed a public property of Ship.cs
---
Commit #7 (5349b86)

Changes:
- Moved some string constants to Utilities.cs
---
Commit #8 (bb2000d)

Changes:
- Refactor board Render()
- Added colour to board Render()
- Improve 'exit' and 'skip' recognition
- Completed salvo functionality
- Fixed Computer v Computer game not showing final move(s)
- Moved 'exit' constant to Utilities.cs
- Changed 'exit' to 'skip' for skipping attack order
---
Commit #9 (cfbd7a4)

Changes:
- Make non-instantiated classes explicitly static
- Changed some constant names
- Cleaning
---
Commit #10 (11c81ac)

Changes:
- Improved randomisation of tile selection
- Refactored and optimised adjacent tile selection
- Added Mines
- Improved console output
- Refactored Controller.cs
- Fixed '5: Quit' not working on main menu
- Renamed 'Orientation.cs' to Direction.cs
- Added Above/Below/Left/Right directions
- Removed redundant Ship.Section parameter in Tile ctor
---
Commit #11 (3edf325)

Changes:
- Added exception handling when parsing config
- Added default values for when a config is malformed
- Added a warning if the config was malformed
- Prevented crash if no name provided for ship type
---
Commit #12 (a41881a)

Changes:
- Fix bug where default ships would always be added
- Reformatted Program.cs
---
Commit #13 (cef8e81)

Changes:
- Adds break when rendering boards on Computer v Computer
---
Commit #14 (2e94085)

Changes:
- Remove inconsistent `_boardName` centering code

### Reflection
Challenge: Getting continuous tiles between two points with minimal code duplication

When originally calling `GetTilesBetween()`, the coordinates of the startTile were compared against the endTile 
to determine the 'direction of movement' (the manipulation of the coordinates to get the next tile). This is
unique in each circumstance below.
 
The startTile was:
- To the right of endTile if `startTile.x > endTile.x` :
    - `x += 1`
- To the left of endTile if `startTile.x < endTile.x` :
    - `x -= 1`
- Below endTile if `startTile.y < endTile.y` :
    - `y += 1`
- Above endTile if `startTile.y > endTile.y` :
    - `y -= 1`

Given the unique values of each branch, four different loops with four different end-conditions and four 
different variable 'steps' are required. The formatted code above outlines them. This led to a very similarly
structured loop with very similar lines of code inside right next to each other.

The solution was found by further breaking down the function `GetBetweenTiles()`; making use of newly added values
to the `Direction` enum. A ternary operator neatly evaluates the four conditions above into just two lines and
stores the result as `Direction.Left/Right/Above/Below`. This is then fed into a new pure function named
`GetNextTile(direction, startTile)` which returns the tile directly adjacent to the start point in the direction
provided - or null if the coordinate is invalid.

Using a while loop, the `GetNextTile()` function is repeatedly called and a variable assigned to its output. When
that variable is equal to endTile, we know we have iterated through each tile between startTile and endTile and
the while loop breaks. This more than halved the `GetBetweenTiles()` function's length, significantly reduced
its complexity and required only a single loop. Additionally, the `GetNextTile()` function was able to be reused
elsewhere. 

## Evaluation
### End-of-Development UML Diagram

![PlantUML Diagram](https://user-images.githubusercontent.com/74011247/139963455-12b6b7c6-41ae-4d81-ad7f-8d72d73daf59.png)

### Reflection
Initial development followed the outlined approach in **Approach** and led to the creation of a 'Project' board 
on GitHub with categorised/tagged 'Issues' serving as the tickets. Creation of the object classes outlined 
in **Initial Design** were completed this way.

Further development utilised a more flexible approach. I determined that maintaining a Kanban board (of-sorts)
as one person did not give me the flexibility to deviate from the plan where appropriate. The initial design
didn't account for complexities and information gathered during the development process. Making edits to
accomodate for this meant also manually updating open tickets/issues on GitHub - unnecessarily increasing
workload. The flexible method - by not updating the GitHub 'Project' or 'Issues' - was definitely preferable
and improved productivity.

Further improvement can be made by more carefully reading the brief. Though my heavy focus on the proposed
program and the problems it presented were beneficial during development, it also resulted in me not properly
understanding the marking criteria; assuming no extensive write-up was needed due to a substitute recorded demo. 
All my efforts were focused on implementing key functionality outlined at the top of the brief and none 
(until the night prior to submission) on the README or design requirements at the bottom. As such, and despite 
otherwise having more data, this document is incomplete.

Given the above and my change in priorities, the final program is also missing two planned features:
- Auto-fire
- Improved AI targeting

Given its nature, an 'Auto-Fire' feature for the `Human` would be very simple to implement; simply reusing the
function the `Computer` uses to attack a tile. Were this to also be improved upon to make the targeting more
intelligent, the 'Auto-Fire' feature would also benefit with no other code changes. The biggest challenge with 
this solution would be the modification and moving of the `Computer`'s targeting function whilst maintaining an
OO-style; ensuring adequate method visibility and grouping (composition). A possible way to deal with this is by
creating a new abstract method in the `IAdmiral` interface named `AutoFire()` and instead calling that method
within the `Computer`'s `RequestAttackCoords()` implementation instead of `GetTileAsInput()`.
