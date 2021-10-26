using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipsAda
{
    public class Controller
    {
        private static Controller _controller;
        
        public static Controller Get() {
            return _controller ??= new Controller();
        }

        private readonly string[] _modeItems = {
            "Player vs Computer", 
            "Computer vs Computer",
            "Player vs Player",
            "Reload Config", 
            "Quit"
        };
        private readonly IAdmiral[] _admirals;
        
        private Tuple<int, int> _boardSize;
        private int _turn;
        private bool _showAiBoards;
        
        public List<ShipInfo> ShipTypes { get; private set; }
        public bool Setup { get; private set; }
        
        private Controller() {
            _controller = this;
            _admirals = new IAdmiral[2];
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
            
            // Prevent out-of-bounds crash if size is greater than 701x701
            if (_boardSize.Item1 > 701 || _boardSize.Item2 > 701) {
                _boardSize = new Tuple<int, int>(701, 701);
            }
        }

        public void Play() {
            _turn = 0;
            int admiralId;
            int enemyId;
            while (true) {
                _turn++;
                admiralId = _turn % 2 == 0 ? 1 : 0;                                                                     // If turn is even, player #2's go
                enemyId = admiralId == 0 ? 1 : 0;
                
                var admiral = _admirals[admiralId];
                if (admiral.IsDefeated()) break;

                Console.Clear();
                Console.WriteLine($"Turn: {_turn}");
                Console.WriteLine($"Player #{admiralId + 1}'s turn...");

                if (_showAiBoards) {
                    if (_admirals[0].GetType() == typeof(Computer)) _admirals[0].Board.Render();
                    if (_admirals[1].GetType() == typeof(Computer)) _admirals[1].Board.Render();
                }
                
                while (true) {
                    var coords = admiral.RequestAttackCoords();
                    if (coords == null) {
                        Console.WriteLine("NO TARGET: Skipping turn...");
                        break;
                    }
                    var formattedCoords = $"{{{Coordinates.GetLetterFromValue(coords.Item1)}, {coords.Item2 + 1}}}";
                    var enemyTile = _admirals[enemyId].Board.GetTileAt(coords);
                    var tBoardTile = admiral.TargetBoard.GetTileAt(coords);
                    
                    string message; 
                    TileState tileState;
                    if (enemyTile.Section != null) {
                        tileState = TileState.Hit;
                        enemyTile.Section.Damaged = true;
                        message = $"HIT: Vessel at {formattedCoords} sustained damaged!";
                    }
                    else {
                        tileState = TileState.Miss;
                        message = $"MISS: No target at {formattedCoords}!";
                    }
                    enemyTile.Attacked = tileState;
                    tBoardTile.Attacked = tileState;
                    Console.WriteLine(message);
                    break;
                }

                Utilities.RequestInput("Press any key to end turn...");
            }
            Console.WriteLine($"{_admirals[admiralId].Name} was defeated!");
            Console.WriteLine($"{_admirals[enemyId].Name} won!");
        }

        private void GameSetup() {
            if (!InitialiseGameMode()) return;
            _admirals[0].SetupFleet();
            _admirals[1].SetupFleet();
        }

        private bool InitialiseGameMode() {
            while (!Setup) {
                Console.Clear();
                Console.WriteLine("Mode Selection:");
                Utilities.OutputList(_modeItems);
                var choice = Utilities.RequestChoice(4);
                switch (choice) {
                    case 1:
                        _admirals[0] = new Human(_boardSize);
                        _admirals[1] = new Computer(_boardSize);
                        Setup = true;
                        break;
                    case 2:
                        _admirals[0] = new Computer(_boardSize, "Computer #1");
                        _admirals[1] = new Computer(_boardSize, "Computer #2");
                        _showAiBoards = true;
                        Setup = true;
                        break;
                    case 3:
                        _admirals[0] = new Human(_boardSize, "Player #1");
                        _admirals[1] = new Human(_boardSize, "Player #2");
                        Setup = true;
                        break;
                    case 4:
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