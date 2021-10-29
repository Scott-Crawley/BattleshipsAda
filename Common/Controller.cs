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

        private readonly string[] _modeItems = {
            "Player vs Computer", 
            "Computer vs Computer",
            "Player vs Player",
            "Reload Config", 
            "Quit"
        };
        private readonly IAdmiral[] _admirals;
        
        // Game Properties
        private Tuple<int, int> _boardSize;
        private int _turn;
        private bool _showAiBoards;
        private bool _quit;
        private bool _salvo;
        private bool _mines;

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

        private void DoTurn(IAdmiral admiral, int enemyId) {
            while (!_quit) {
                var coords = admiral.RequestAttackCoords();
                if (coords == null || _quit) {
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
                    if (enemyTile.Section.Ship.Destroyed) {
                        message = $"SUNK: Vessel at {formattedCoords} has been sunk!";
                    }
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
        }

        private void DoSalvoTurn(IAdmiral admiral, int enemyId) {
            var ships = admiral.Fleet.Ships;
            var aliveShips = ships.Length - admiral.Fleet.DestroyedShips;
            var shipsFired = 0;
            foreach (var ship in ships) {                                                                               // For each non-destroyed ship, allow attacking a tile
                if (_quit || _admirals[enemyId].IsDefeated()) break;
                if (ship.Destroyed) continue;
                
                DoTurn(admiral, enemyId);
                Console.WriteLine($"SHIP: {ship.Name} completed attack order!");
                shipsFired++;

                if (shipsFired == aliveShips) break;                                                                    // Don't ask for input if last ship
                if (admiral.GetType() == typeof(Computer)) continue;                                                    // Don't ask for input if Computer
                
                var input = Utilities.RequestInput("\nPress any key to order next ship... " +
                                                                "\nType 'exit' to quit: ");
                if (input.ToLower() == Utilities.EXIT) _quit = true;
            }
        }

        public void Play() {
            var enemyId   = 0;
            var admiralId = 0;

            _quit  = false;
            _turn = 0;
            while (!_quit) {
                _turn++;
                admiralId = _turn % 2 == 0 ? 1 : 0;                                                                     // If turn is even, player #2's go
                enemyId   = admiralId == 0 ? 1 : 0;
                
                Console.Clear();
                Console.WriteLine($"Turn: {_turn}");
                Console.WriteLine($"Player #{admiralId + 1}'s turn...");
                
                if (_showAiBoards) {
                    if (_admirals[0].GetType() == typeof(Computer)) _admirals[0].Board.Render();
                    if (_admirals[1].GetType() == typeof(Computer)) _admirals[1].Board.Render();
                }
                
                var admiral = _admirals[admiralId];
                if (admiral.IsDefeated()) break;
                
                if (_salvo) DoSalvoTurn(admiral, enemyId);
                else DoTurn(admiral, enemyId);

                if (_quit) break;
                var input = Utilities.RequestInput("\nPress any key to end turn... \nType 'exit' to quit: ");
                if (input.ToLower() == Utilities.EXIT) _quit = true;
            }

            if (_quit) {
                Console.WriteLine("Game cancelled");
                return;
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
                        SubModePrompt();
                        break;
                    case 2:
                        _admirals[0] = new Computer(_boardSize, "Computer #1");
                        _admirals[1] = new Computer(_boardSize, "Computer #2");
                        SubModePrompt();
                        _showAiBoards = true;
                        break;
                    case 3:
                        _admirals[0] = new Human(_boardSize, "Player #1");
                        _admirals[1] = new Human(_boardSize, "Player #2");
                        SubModePrompt();
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

        private void SubModePrompt() {
            while (true) {
                Console.Clear();
                Console.WriteLine("Sub-Mode Selection:");

                var salvoOption = "Salvo  " + (_salvo ? Utilities.PLACED_STR : Utilities.UNPLACED_STR);
                var minesOption = "Mines  " + (_mines ? Utilities.PLACED_STR : Utilities.UNPLACED_STR);
                var subModeOptions = new[] { salvoOption, minesOption, "Done" };
                Utilities.OutputList(subModeOptions);
                
                var choice = Utilities.RequestChoice(3);
                switch (choice) {
                    case 1:
                        _salvo = !_salvo;
                        break;
                    case 2:
                        _mines = !_mines;
                        break;
                    case 3:
                        Setup = true;
                        return;
                }
            }
        }
    }
}