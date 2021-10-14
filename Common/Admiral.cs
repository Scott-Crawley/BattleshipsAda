using System;

namespace BattleshipsAda
{
    public class Admiral
    {
        private readonly PlayerType _playerType;
        private readonly Fleet _fleet;

        public Board Board { get; }

        public Board TargetBoard { get; }

        public Admiral(PlayerType playerType, Tuple<int, int> boardSize) {
            _playerType = playerType;
            _fleet = new Fleet(this);
            Board = new Board(boardSize);
            TargetBoard = new Board(boardSize);
        }
    }
}