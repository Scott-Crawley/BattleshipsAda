using System;

namespace BattleshipsAda
{
    public class Computer : IAdmiral
    {
        private const string NAME = "Computer";
        
        private readonly Random _random;
        
        private static readonly Func<Board.Tile, bool> FreeTileCriteria = tile => tile.Section == null;
        private static readonly Func<Board.Tile, bool> NotAttackedCriteria = tile => tile.Attacked == TileState.None;

        public string Name { get; }
        public Board Board { get; }
        public Board TargetBoard { get; }
        public Fleet Fleet { get; }

        public Computer(Tuple<int, int> boardSize, string name = null) {
            Name = name ?? NAME;
            Board = new Board(boardSize, $"{Name} Board");
            TargetBoard = new Board(boardSize, $"{Name} Target Board");
            Fleet = new Fleet(this);
            _random = new Random();
        }

        public void SetupFleet() {
            AutoPlaceShips();
        }

        public Tuple<int, int> RequestAttackCoords() {
            return GetTileAsInput(TargetBoard, NotAttackedCriteria).Coords;
        }
        
        public bool IsDefeated() {
            return Fleet.DestroyedShips == Fleet.Ships.Length;
        }

        private static Board.Tile GetTileAsInput(Board board, Func<Board.Tile, bool> matchCriteria) {
            return board.GetRandomTile(matchCriteria);
        }
        
        private void AutoPlaceShips(bool allShips = true) {
            var ships = allShips ? Fleet.Ships : Fleet.GetUnplacedShips();
            foreach (var ship in ships) {
                var placed = false;
                while (!placed) {
                    var orientation = RandomOrientation();
                    placed = Fleet.PlaceShip(ship, GetTileAsInput(Board, FreeTileCriteria), orientation);
                }
            }
        }

        private Direction RandomOrientation() {
            return _random.Next(1, 10) % 2 == 0 ? Direction.Vertical : Direction.Horizontal;
        }
    }
}