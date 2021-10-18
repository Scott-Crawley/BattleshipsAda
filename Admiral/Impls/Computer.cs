using System;

namespace BattleshipsAda
{
    public class Computer : IAdmiral
    {
        private readonly Random _random;
        private readonly Fleet _fleet;
        
        public Board Board { get; }
        public Board TargetBoard { get; }

        public Computer(Tuple<int, int> boardSize) {
            Board = new Board(boardSize);
            TargetBoard = new Board(boardSize);
            _fleet = new Fleet(this);
            _random = new Random();
        }

        public void SetupFleet() {
            AutoPlaceShips();
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