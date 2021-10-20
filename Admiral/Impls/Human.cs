using System;
using System.Linq;

namespace BattleshipsAda
{
    public class Human : IAdmiral
    {
        private const string UNPLACED = "[_]";
        private const string PLACED = "[x]";
        
        private readonly Random _random;
        private readonly Fleet _fleet;
        private readonly string[] _setupItems = { "Place Ship", "Unplace Ship", "Autoplace All", "Autoplace Remaining", "Reset" };
        
        public Board Board { get; }
        public Board TargetBoard { get; }
        
        public Human(Tuple<int, int> boardSize) {
            Board = new Board(boardSize, "[Human] Board");
            TargetBoard = new Board(boardSize, "[Human] Target Board");
            _fleet = new Fleet(this);
            _random = new Random();
        }

        public void SetupFleet() {
            var completedSetup = false;
            while (!completedSetup) {
                Console.Clear();
                Board.Render();
                Console.WriteLine("Fleet Setup:");
                Utilities.OutputList(_setupItems);

                var enableContinue = _fleet.GetPlacedShips().Length == _fleet.Ships.Length;
                if (enableContinue) Console.WriteLine("6: Start Game");
                
                var choice = Utilities.RequestChoice(6);
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
                        foreach (var ship in _fleet.GetPlacedShips()) {
                            _fleet.UnplaceShip(ship);
                        }
                        break;
                    case 6:
                        if (enableContinue) {
                            completedSetup = true;
                            break;
                        }
                        Console.WriteLine("You must place all ships before continuing.");
                        break;
                }
            }
        }
        
        public void DoTurn() {
            throw new NotImplementedException();
        }
        
        public bool IsDefeated() {
            return _fleet.DestroyedShips == _fleet.Ships.Length;
        }

        public void AttackTile(Board.Tile tile) {
            throw new NotImplementedException();
        }

        public Board.Tile GetTileAsInput() {
            var tileInvalidMsg = false;
            var coordInvalidMsg = false;
            Board.Tile tile = null;

            while (tile == null) {
                Console.Clear();
                Board.Render();
                Console.WriteLine("Enter 'exit' to cancel");
                if (tileInvalidMsg) Console.WriteLine("Tile not found!");
                if (coordInvalidMsg) Console.WriteLine("Invalid Coordinates!"); 
                tileInvalidMsg = false;
                coordInvalidMsg = false;
                
                var inX = Utilities.RequestInput("X Coordinate: ").ToLower();
                if (inX == "exit") break;
                var x = Coordinates.GetLetterCoordValue(inX);
                
                var inY = Utilities.RequestInput("Y Coordinate: ").ToLower();
                if (inY == "exit") break;
                var validY = int.TryParse(inY, out var y);
                
                if (x == -1 || !validY) {
                    coordInvalidMsg = true;
                    continue;
                }

                tile = Board.GetTileAt(new Tuple<int, int>(x, y));
                if (tile == null) tileInvalidMsg = true;
            }
            return tile;
        }

        public void AutoPlaceShips(bool allShips = true) {
            var ships = allShips ? _fleet.Ships : _fleet.GetUnplacedShips();
            foreach (var ship in ships) {
                _fleet.UnplaceShip(ship);                                                                               // Optimises tile selection for smaller boards 
                var placed = false;
                while (!placed) {
                    var orientation = RandomOrientation();
                    placed = _fleet.PlaceShip(ship, Board.GetRandomFreeTile(), orientation);
                }
            }
        }

        private void PlacementMode() {
            while (true) {
                var ships = _fleet.Ships;
                Console.Clear();
                Board.Render();
                Utilities.OutputList(ships.Select(ship => {                                                       // List output formatting to show if a ship is (un)placed
                    var icon = ship.Placed ? PLACED : UNPLACED;
                    return $"{ship.Name,-15} {icon}";
                }));
                Console.WriteLine("0: Cancel");
                
                var shipChoice = Utilities.RequestChoice(ships.Length);
                if (shipChoice == 0) return;
                
                var tile = GetTileAsInput();
                if (tile == null) continue;
                
                Utilities.OutputList(new [] { "Horizontal", "Vertical" });
                
                var oriChoice = Utilities.RequestChoice(2);
                var orientation = oriChoice == 1 ? Orientation.Horizontal : Orientation.Vertical;

                _fleet.PlaceShip(ships[shipChoice - 1], tile, orientation);
            }
        }

        private void ResetShipMode() {
            while (true) {
                Console.Clear();
                Board.Render();
                Console.WriteLine("Enter 'exit' to cancel");
                
                var input = Utilities.RequestInput("Enter ship initial/name: ").ToLower();
                if (input == "exit") return;
                
                foreach (var ship in _fleet.GetPlacedShips()) {
                    var shipName = ship.Name.ToLower();
                    if (input == shipName || input[0] == shipName[0]) {
                        _fleet.UnplaceShip(ship);
                    }
                }
            }
        }
        
        private Orientation RandomOrientation() {
            return _random.Next(1, 10) % 2 == 0 ? Orientation.Vertical : Orientation.Horizontal;
        }
    }
}