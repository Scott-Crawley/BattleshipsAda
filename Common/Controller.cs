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

        private int _turn;
        private readonly string[] _modeItems = { "One player vs Computer", "Reload Config", "Quit" };
        private Tuple<int, int> _boardSize;
        public List<ShipInfo> ShipTypes { get; private set; }
        public IAdmiral[] Admirals { get; }
        public bool Setup { get; private set; }
        
        private Controller() {
            _controller = this;
            Admirals = new IAdmiral[2];
            LoadConfiguration();
            GameSetup();
        }

        private void LoadConfiguration() {
            var config = Config.LoadConfiguration();
            if (config.BoardSize == null || config.ShipTypes == null) {
                Console.WriteLine("Malformed Configuration!");
                return;
            }
            
            ShipTypes = config.ShipTypes;
            _boardSize = config.BoardSize;
        }

        public void Play() {
            _turn = 1;
            int admiralId;
            while (true) {
                admiralId = _turn % 2 == 0 ? 1 : 0;                                                                     // If turn is even, player #2's go
                var admiral = Admirals[admiralId];
                admiral.DoTurn();
                if (admiral.IsDefeated()) {
                    break;
                }
            }
            Console.WriteLine($"Player #{admiralId + 1} was defeated!");
        }

        private void GameSetup() {
            if (!InitialiseGamemode()) return;
            Admirals[0].SetupFleet();
            Admirals[1].SetupFleet();
        }

        private bool InitialiseGamemode() {
            while (!Setup) {
                Console.Clear();
                Console.WriteLine("Mode Selection:");
                Utilities.OutputList(_modeItems);
                var choice = Utilities.RequestChoice(4);
                switch (choice) {
                    case 1:
                        Admirals[0] = new Human(_boardSize);
                        Admirals[1] = new Computer(_boardSize);
                        Setup = true;
                        break;
                    case 2:
                        LoadConfiguration();
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}