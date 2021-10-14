using System;
using System.Collections.Generic;

namespace BattleshipsAda
{
    public class Controller
    {
        private static Controller _controller;

        public static Controller Get() {
            return _controller ??= new Controller();
        }
        
        private readonly Tuple<int, int> _boardSize;
        public List<ShipType> ShipTypes { get; }
        public List<Admiral> Admirals { get; }

        private Controller() {
            var config = Config.LoadConfiguration();
            ShipTypes = config.ShipTypes;
            _boardSize = config.BoardSize;
            GameSetup();
            
            _controller = this;
        }

        private void GameSetup() {
            Admirals.Add(new Admiral(PlayerType.HUMAN, _boardSize));
            Admirals.Add(new Admiral(PlayerType.COMPUTER, _boardSize));
        }
    }
}