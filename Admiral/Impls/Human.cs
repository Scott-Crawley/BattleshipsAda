using System;

namespace BattleshipsAda
{
    public class Human : IAdmiral
    {
        private readonly Random _random;
        private readonly Fleet _fleet;
        private readonly string[] _setupItems = { "Place Ship", "Autoplace All", "Autoplace Remaining", "Reset", "Start Game" };
        
        public Board Board { get; }
        public Board TargetBoard { get; }
        
        public Human(Tuple<int, int> boardSize) {
            Board = new Board(boardSize);
            TargetBoard = new Board(boardSize);
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
                
                var choice = Utilities.RequestChoice(5);
                switch (choice) {
                    case 1:
                        Console.WriteLine("Unimplemented");
                        break;
                    case 2:
                        AutoPlaceShips();
                        break;
                    case 3:
                        AutoPlaceShips(false);
                        break;
                    case 4:
                        foreach (var ship in _fleet.GetPlacedShips()) {
                            _fleet.UnplaceShip(ship);
                        }
                        break;
                    case 5:
                        if (_fleet.GetPlacedShips().Length == _fleet.Ships.Length) {
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

        public Board.Tile GetTileToAttack() {
            throw new NotImplementedException();
        }

        public void AutoPlaceShips(bool allShips = true) {
            var ships = allShips ? _fleet.Ships : _fleet.GetUnplacedShips();
            foreach (var ship in ships) {
                var placed = false;
                while (!placed) {
                    var orientation = RandomOrientation();
                    placed = _fleet.PlaceShip(ship, Board.GetRandomFreeTile(), orientation);
                }
            }
        }
        
        private Orientation RandomOrientation() {
            return _random.Next(1, 10) % 2 == 0 ? Orientation.VERTICAL : Orientation.HORIZONTAL;
        }
    }
}