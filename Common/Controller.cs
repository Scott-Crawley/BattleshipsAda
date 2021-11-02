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
        private bool _warn;

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

            ShipTypes = config.ShipTypes;
            _boardSize = config.BoardSize;
            _warn = config.WarnMalformed;
            
            // Prevent out-of-bounds crash if size is greater than 701x701
            if (_boardSize.Item1 > 701 || _boardSize.Item2 > 701) {
                _boardSize = new Tuple<int, int>(701, 701);
            }
        }

        private void DoTurn(IAdmiral attacker, IAdmiral defender) {
            while (!_quit) {
                var coords = attacker.RequestAttackCoords();
                if (coords == null || _quit) {
                    Console.WriteLine("NO TARGET: Skipping turn...");
                    break;
                }
                var formattedCoords = $"{{{Coordinates.GetLetterFromValue(coords.Item1)}, {coords.Item2 + 1}}}";
                var defenderTile = defender.Board.GetTileAt(coords);
                var attackerTgrt = attacker.TargetBoard.GetTileAt(coords);
                
                string message; 
                TileState tileState;
                var mine = defenderTile.Mine;
                var section = defenderTile.Section;
                
                if (section != null) {
                    var ship = section.Ship;
                    tileState = TileState.Hit;
                    section.Damaged = true;
                    message = ship.Destroyed
                        ? $"SUNK: {defender.Name}'s {ship.Name} at {formattedCoords} has been sunk!"
                        : $"HIT : {defender.Name}'s {ship.Name} at {formattedCoords} sustained damaged!";
                }
                else if (mine != null && !mine.Exploded) {
                    defender.Board.ExplodeMine(defenderTile);
                    tileState = TileState.Hit;
                    message = $"MINE: A naval mine exploded at {formattedCoords}; damaging all adjacent tiles!";
                }
                else {
                    tileState = TileState.Miss;
                    message = $"MISS: No target at {formattedCoords}!";
                }
                defenderTile.Attacked = tileState;
                attackerTgrt.Attacked = tileState;
                Console.WriteLine(message);
                break;
            }
        }

        private void DoSalvoTurn(IAdmiral attacker, IAdmiral defender) {
            var ships = attacker.Fleet.Ships;
            var aliveShips = ships.Length - attacker.Fleet.DestroyedShips;
            var shipsFired = 0;
            
            Console.WriteLine($"FLEET: Ships available: {aliveShips}\n");
            foreach (var ship in ships) {                                                                               // For each non-destroyed ship, allow attacking a tile
                if (_quit || defender.IsDefeated()) break;
                if (ship.Destroyed) continue;
                
                DoTurn(attacker, defender);
                shipsFired++;

                if (shipsFired == aliveShips) break;                                                                    // Don't ask for input if last ship
                if (attacker.GetType() == typeof(Computer)) continue;                                                   // Don't ask for input if Computer
                
                var input = Utilities.RequestInput("\nPress any key to order next ship... " +
                                                                "\nType 'exit' to quit: ");
                if (input.ToLower() == Utilities.EXIT) _quit = true;
            }
        }

        public void Play() {
            var defenderId = 0;
            var attackerId = 0;

            _quit = false;
            _turn = 0;
            while (!_quit) {
                _turn++;
                attackerId = _turn % 2 == 0 ? 1 : 0;                                                                    // If turn is even, player #2's go
                defenderId = attackerId == 0 ? 1 : 0;
                var attacker = _admirals[attackerId];
                var defender = _admirals[defenderId];
                
                Console.Clear();
                if (_showAiBoards) {
                    if (_admirals[0].GetType() == typeof(Computer)) _admirals[0].Board.Render();
                    if (_admirals[1].GetType() == typeof(Computer)) _admirals[1].Board.Render();
                    Console.WriteLine();
                }
                
                Console.WriteLine($"Turn: [{_turn}]");
                Console.WriteLine($"{attacker.Name} is attacking...");
                
                if (attacker.IsDefeated()) break;
                if (_salvo) DoSalvoTurn(attacker, defender);
                else DoTurn(attacker, defender);

                if (_quit) break;
                var input = Utilities.RequestInput("\nPress any key to end turn... \nType 'exit' to quit: ");
                if (input.ToLower() == Utilities.EXIT) _quit = true;
            }

            if (_quit) {
                Console.WriteLine("Game cancelled");
                return;
            }
            Console.WriteLine($"{_admirals[attackerId].Name} was defeated!");
            Console.WriteLine($"{_admirals[defenderId].Name} won!");
        }

        private void GameSetup() {
            if (!InitialiseGameMode()) return;
            _admirals[0].SetupFleet();
            _admirals[1].SetupFleet();
            if (_mines) PlaceMines();
        }

        private bool InitialiseGameMode() {
            while (!Setup) {
                Console.Clear();

                if (_warn) {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("WARN: Malformed adaship_config.ini: using defaults");
                    Console.ResetColor();
                }
                
                Console.WriteLine("Mode Selection:");
                Utilities.OutputList(_modeItems);
                var choice = Utilities.RequestChoice(5);
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

                var salvoOption = "Salvo  " + (_salvo ? Utilities.ENABLE_STR : Utilities.DISABLE_STR);
                var minesOption = "Mines  " + (_mines ? Utilities.ENABLE_STR : Utilities.DISABLE_STR);
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

        private void PlaceMines() {
            for (var i = 0; i < 5; i++) {
                static bool MatchCriteria(Board.Tile tile) => tile.Section == null && tile.Mine == null;                // Local function for matchCriteria
                _admirals[0].Board.GetRandomTile(MatchCriteria).Mine = new Board.Mine();
                _admirals[1].Board.GetRandomTile(MatchCriteria).Mine = new Board.Mine();
            }
        }
    }
}