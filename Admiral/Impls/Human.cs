using System;
using System.Linq;

namespace BattleshipsAda
{
    public class Human : IAdmiral
    {
        private const string NAME = "Player";

        private readonly Random _random;
        private readonly string[] _setupItems = {
            "Place Ship",
            "Unplace Ship",
            "Autoplace All",
            "Autoplace Remaining",
            "Reset"
        };
        private readonly string[] _errors = {
            "Invalid Coordinates!",
            "Tile not found!",
            "Coordinates already attacked!"
        };

        private static readonly Func<Board.Tile, bool> FreeTileCriteria = tile => tile.Section == null;
        
        public string Name { get; }
        public Board Board { get; }
        public Board TargetBoard { get; }
        public Fleet Fleet { get; }

        public Human(Tuple<int, int> boardSize, string name = null) {
            Name = name ?? NAME;
            Board = new Board(boardSize, $"{Name} Board");
            TargetBoard = new Board(boardSize, $"{Name} Target Board");
            Fleet = new Fleet(this);
            _random = new Random();
        }

        public void SetupFleet() {
            var completedSetup = false;
            while (!completedSetup) {
                Board.Render(true);
                Console.WriteLine("Fleet Setup:");
                Utilities.OutputList(_setupItems);

                var maxIndex = _setupItems.Length + 1;
                var enableContinue = Fleet.GetPlacedShips().Length == Fleet.Ships.Length;                        // Ensure all ships are placed before allowing 'Start Game'
                if (enableContinue) Console.WriteLine($"{maxIndex}: Start Game");
                
                var choice = Utilities.RequestChoice(maxIndex);
                switch (choice) {
                    case 1:
                        PlacementMode();
                        break;
                    case 2:
                        ResetShipMode();
                        break;
                    case 3:
                        AutoPlaceShips();
                        break;
                    case 4:
                        AutoPlaceShips(false);
                        break;
                    case 5:
                        foreach (var ship in Fleet.GetPlacedShips()) {
                            Fleet.UnplaceShip(ship);
                        }
                        break;
                }
                if (choice == maxIndex && enableContinue) completedSetup = true;                                        // Handle 'Start Game' (can't use non-const as switch case)
                else Console.WriteLine("You must place all ships before continuing.");
            }
        }
        
        public Tuple<int, int> RequestAttackCoords() {
            TargetBoard.Render(true);
            Board.Render();
            return GetTileAsInput(TargetBoard)?.Coords;
        }
        
        public bool IsDefeated() {
            return Fleet.DestroyedShips == Fleet.Ships.Length;
        }

        private Board.Tile GetTileAsInput(Board board) {
            var errorMsgIndex = -1;
            Board.Tile tile = null;

            while (tile == null) {
                Console.WriteLine("Enter 'skip' to skip attack");
                if (errorMsgIndex > -1) Console.WriteLine(_errors[errorMsgIndex]);
                errorMsgIndex = -1;
                
                var inX = Utilities.RequestInput("X Coordinate: ").ToLower();
                if (inX == Utilities.SKIP) break;
                var x = Coordinates.GetValueFromLetter(inX);
                
                var inY = Utilities.RequestInput("Y Coordinate: ").ToLower();
                if (inY == Utilities.SKIP) break;
                var validY = int.TryParse(inY, out var y);
                
                if (x == -1 || !validY) {
                    errorMsgIndex = 0;
                    continue;
                }

                tile = board.GetTileAt(new Tuple<int, int>(x, y - 1));                                            // Minus one since we display the Y-axis starting from 1
                if (tile != null) {
                    if (tile.Attacked == TileState.None) {
                        continue;
                    }
                    tile = null;
                    errorMsgIndex = 2;
                }
                else errorMsgIndex = 1;
            }
            return tile;
        }

        private void AutoPlaceShips(bool allShips = true) {
            var ships = allShips ? Fleet.Ships : Fleet.GetUnplacedShips();
            foreach (var ship in ships) {
                Fleet.UnplaceShip(ship);                                                                                // Optimises tile selection for smaller boards 
                var placed = false;
                while (!placed) {
                    var orientation = RandomOrientation();
                    placed = Fleet.PlaceShip(ship, Board.GetRandomTile(FreeTileCriteria), orientation);
                }
            }
        }

        private void PlacementMode() {
            while (true) {
                var ships = Fleet.Ships;
                Board.Render(true);
                Utilities.OutputList(ships.Select(ship => {                                                       // List output formatting to show if a ship is (un)placed
                    var icon = ship.Placed ? Utilities.PLACED_STR : Utilities.UNPLACED_STR;
                    return $"{ship.Name,-15} {icon}";
                }));
                Console.WriteLine("0: Cancel");
                
                var shipChoice = Utilities.RequestChoice(ships.Length);
                if (shipChoice == 0) return;
                
                Board.Render(true);
                var tile = GetTileAsInput(Board);
                if (tile == null) continue;
                
                Utilities.OutputList(new [] { "Horizontal", "Vertical" });
                
                var oriChoice = Utilities.RequestChoice(2);
                var orientation = oriChoice == 1 ? Orientation.Horizontal : Orientation.Vertical;

                Fleet.PlaceShip(ships[shipChoice - 1], tile, orientation);
            }
        }

        private void ResetShipMode() {
            while (true) {
                Board.Render(true);
                Console.WriteLine("Enter 'exit' to cancel");
                
                var input = Utilities.RequestInput("Enter ship initial/name: ").ToLower();
                if (input == Utilities.EXIT) return;
                
                foreach (var ship in Fleet.GetPlacedShips()) {
                    var shipName = ship.Name.ToLower();
                    if (input == shipName || input[0] == shipName[0]) {
                        Fleet.UnplaceShip(ship);
                    }
                }
            }
        }
        
        private Orientation RandomOrientation() {
            return _random.Next(1, 10) % 2 == 0 ? Orientation.Vertical : Orientation.Horizontal;
        }
    }
}