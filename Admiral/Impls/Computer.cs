using System;

namespace BattleshipsAda
{
    public class Computer : IAdmiral
    {
        private const string NAME = "Computer";
        
        private readonly Random _random;
        private readonly Fleet _fleet;
        
        private static readonly Func<Board.Tile, bool> FreeTileCriteria = tile => tile.Section == null;
        private static readonly Func<Board.Tile, bool> NotAttackedCriteria = tile => tile.Attacked == TileState.None;

        public string Name { get; }
        public Board Board { get; }
        public Board TargetBoard { get; }

        public Computer(Tuple<int, int> boardSize, string name = null) {
            Board = new Board(boardSize, $"{Name} Board");
            TargetBoard = new Board(boardSize, $"{Name} Target Board");
            _fleet = new Fleet(this);
            _random = new Random();
            Name = name ?? NAME;
        }

        public void SetupFleet() {
            AutoPlaceShips();
        }

        public Tuple<int, int> RequestAttackCoords() {
            return GetTileAsInput(TargetBoard, NotAttackedCriteria).Coords;
        }
        
        public bool IsDefeated() {
            return _fleet.DestroyedShips == _fleet.Ships.Length;
        }

        private static Board.Tile GetTileAsInput(Board board, Func<Board.Tile, bool> matchCriteria) {
            return board.GetRandomTile(matchCriteria);
        }
        
        private void AutoPlaceShips(bool allShips = true) {
            var ships = allShips ? _fleet.Ships : _fleet.GetUnplacedShips();
            foreach (var ship in ships) {
                var placed = false;
                while (!placed) {
                    var orientation = RandomOrientation();
                    placed = _fleet.PlaceShip(ship, GetTileAsInput(Board, FreeTileCriteria), orientation);
                }
            }
        }

        private Orientation RandomOrientation() {
            return _random.Next(1, 10) % 2 == 0 ? Orientation.Vertical : Orientation.Horizontal;
        }
    }
}