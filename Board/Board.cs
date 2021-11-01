using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipsAda
{
    public class Board
    {
        private readonly Tuple<int, int> _size;
        private readonly int _totalSize;
        private readonly string _boardName;
        private readonly bool _warnSize;
        private readonly Random _random;

        public Tile[] Tiles { get; }

        public Board(Tuple<int, int> size, string name) {
            _boardName = name;
            _size = size;
            _totalSize = _size.Item1 * _size.Item2;
            if (_totalSize > 900) {                                                                                     // 900 = 30x30
                _warnSize = true;
            }
            Tiles = new Tile[_totalSize];
            _random = new Random();
            PopulateTiles();
        }
        
        
        // ############### PRIVATE METHODS ############### //
        private void PopulateTiles() {
            var (maxX, y) = _size;                                                                              // Start at max size coord to get ascending Y-axis
            var coord = new Tuple<int, int>(0, y - 1);                                                                  // Zero-based array index, so minus 1 from max size
            for (var i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new Tile(coord);
                coord = Coordinates.GetNextCoord(maxX - 1, coord);                                                // As above
            }
        }

        private string GenerateXAxis() {
            var axisStr = $"{" ",5}";
            var total     = 0;
            var index     = 0;
            var wraps = -1;
            
            while (total < _size.Item1) {
                if (index >= 26) {                                                                                      // Array wrapping
                    index = 0;
                    wraps++;
                }

                var padding = "  ";
                if (wraps >= 0) {
                    axisStr += Coordinates.ALPHABET[wraps];                                                             // Wrapping allows for theoretical max size of 701x701 (ZZ)
                    padding = " ";                                                                                      // 1 less pad for double-size label
                }
                axisStr += Coordinates.ALPHABET[index] + padding;
                total++; 
                index++;
            }
            return axisStr;
        }
        
        private Tile GetNextTile(Direction direction, Tile centre) {
            if (centre?.Coords == null) return null;
            
            var (x, y) = centre.Coords;
            switch (direction) {
                case Direction.Above:
                    y += 1;
                    break;
                case Direction.Below:
                    y -= 1;
                    break;
                case Direction.Left:
                    x -= 1;
                    break;
                case Direction.Right:
                    x += 1;
                    break;
            }
            var tile = GetTileAt(new Tuple<int, int>(x, y));
            return tile == centre ? null : tile;                                                                        // If we're returning 'centre', make it null 
        }
        
        private IEnumerable<Tile> GetAdjacentTiles(Tile centre) {
            var tiles = new List<Tile> {
                GetNextTile(Direction.Left, centre),
                GetNextTile(Direction.Right, centre)
            };
            var above = GetNextTile(Direction.Above, centre);
            var below = GetNextTile(Direction.Below, centre);
            
            // Corners
            var abLeft  = GetNextTile(Direction.Left, above);
            var abRight = GetNextTile(Direction.Right, above);
            var beLeft  = GetNextTile(Direction.Left, below);
            var beRight = GetNextTile(Direction.Right, below);

            tiles.AddRange(new[] { above, below, abLeft, abRight, beLeft, beRight });
            return tiles;
        }
        
        private IEnumerable<Tile> GetTilesBetween(Tile startTile, Tile endTile, Direction direction) {
            if (startTile == null || endTile == null) return null;
            if (!(direction == Direction.Horizontal || direction == Direction.Vertical)) return null;
            
            var tiles = new List<Tile>();
            var (startX, startY) = startTile.Coords;
            var (endX, endY)     = endTile.Coords;

            Direction stepDirection;
            if (direction == Direction.Horizontal) {
                stepDirection = startX > endX ? Direction.Left : Direction.Right;
            }
            else {
                stepDirection = startY > endY ? Direction.Below : Direction.Above;
            }

            tiles.Add(startTile);                                                                                       // Make sure to add the starting tile
            
            var curTile = startTile;
            while (curTile != endTile) {
                curTile = GetNextTile(stepDirection, curTile);
                tiles.Add(curTile);
            }
            return tiles.ToArray();
        }

        // ############### PUBLIC METHODS ############### //
        public void Render(bool clear = false) {
            if (clear) Console.Clear();
            Console.WriteLine();
            Console.WriteLine(_boardName.PadLeft((_boardName.Length + (_size.Item1 * 3 / 4) + _size.Item1) / 2));       // Centre is roughly calculated as this somehow ¯\_(ツ)_/¯
            
            var tileNo = 0;
            for (var row = _size.Item2; row > 0; row--) {                                                           // Decrement to get ascending Y-axis
                Console.Write($"{row,3}");
                
                for (var col = 0; col < _size.Item1; col++) {
                    var tile = Tiles[tileNo];
                    var section = tile.Section;
                    
                    char boardChar;
                    ConsoleColor colour;
                    switch (tile.Attacked) {
                        case TileState.Hit:
                            boardChar = 'X';
                            colour = ConsoleColor.Red;
                            break;
                        case TileState.Miss:
                            boardChar = '-';
                            colour = ConsoleColor.DarkCyan;
                            break;
                        default:
                            boardChar = '.';
                            colour = ConsoleColor.DarkGray;
                            break;
                    }
                    if (section != null) {                                                                              // Section always null on Target Boards; won't run (good!)
                        boardChar = section.Damaged ? 'X' : section.Ship.Name[0];                                       // If a section and not damaged, use the ship's initial
                        colour = section.Damaged ? ConsoleColor.Red : ConsoleColor.White;
                    }

                    if (tile.Mine != null) {
                        boardChar = 'M';
                        colour = tile.Mine.Exploded ? ConsoleColor.DarkRed : ConsoleColor.DarkYellow;
                    }

                    Console.ForegroundColor = colour;
                    Console.Write($"{boardChar,3}");
                    Console.ResetColor();
                    tileNo++;
                }
                Console.WriteLine();
            }
            
            Console.WriteLine(GenerateXAxis());
            if (_warnSize) Console.WriteLine("WARN: Boards over 30x30 may introduce graphical issues\n");               // *Works* up to 701x701.. usability just not guaranteed
        }

        public Tile[] FindContinuousTilesAt(Tile startTile, Direction direction, int numTiles) {
            if (startTile == null)                     return null;
            if (direction == Direction.None)           return null;
            if (numTiles < 0 || numTiles > _totalSize) return null;

            var (x, y) = startTile.Coords;
            var endTileCoords = new Tuple<int, int>(0, 0);
            for (var i = 0; i < 4; i++) {
                if (direction == Direction.Vertical && (i == 0 || i == 1)) continue;                                    // Don't calculate tiles for both orientations
                if (direction == Direction.Horizontal && i == 2) break;
                
                endTileCoords = i switch {                                                                              // -1 to numTiles because of zero-based array index
                    0 => new Tuple<int, int>(x + numTiles - 1, y),                                                      // EAST
                    1 => new Tuple<int, int>(x - numTiles - 1, y),                                                      // WEST
                    2 => new Tuple<int, int>(x, y + numTiles - 1),                                                      // NORTH
                    3 => new Tuple<int, int>(x, y - numTiles - 1),                                                      // SOUTH
                    _ => endTileCoords
                };
                var endTile = GetTileAt(endTileCoords);
                if (endTile?.Section != null) continue;                                                                 // Ensure endTile is free (and not null)
                
                var tiles = GetTilesBetween(startTile, endTile, direction);
                if (tiles != null && tiles.All(tile => tile != null && tile.Section == null)) {                 // Ensure tiles in-between are free (and not null)
                    return (Tile[]) tiles;
                }
            }
            return null;
        }

        public bool UnclaimTiles(Tile startTile, Tile endTile, Direction direction) {
            var tiles = GetTilesBetween(startTile, endTile, direction);
            var tileCount = 0;
            
            foreach (var tile in tiles) {
                tileCount++;
                tile.Section = null;
            }
            return tileCount >= 1;
        }

        public Tile GetTileAt(Tuple<int, int> coords) {
            return !Coordinates.CoordsValid(_size, coords) 
                ? null 
                : Tiles.FirstOrDefault(tile => Equals(tile.Coords, coords));
        }

        public Tile GetRandomTile(Func<Tile, bool> matchCriteria = null) {                                              // Predicate for custom matching criteria
            IEnumerable<Tile> tiles = Tiles;
            if (matchCriteria != null) {
                tiles = Tiles.Where(matchCriteria);
            }
            
            return tiles.ElementAt(_random.Next(0, tiles.Count()));
        }

        public void ExplodeMine(Tile centre) {
            centre.Mine.Exploded = true;
            var adjacentTiles = GetAdjacentTiles(centre);
            foreach (var tile in adjacentTiles) {
                if (tile == null) continue;
                
                var tileState = TileState.Miss;
                if (tile.Mine != null && !tile.Mine.Exploded) {
                    ExplodeMine(tile);                                                                                  // Chain explode if another mine - uses recursion
                }

                if (tile.Section != null) {
                    tile.Section.Damaged = true;
                    tileState = TileState.Hit;
                }
                tile.Attacked = tileState;
            }
        }
        
        // ############### BOARD.TILE ############### //
        public class Tile
        {
            public Tile(Tuple<int,int> coords) {
                Coords = coords;
                Attacked = TileState.None;
            }

            public Tuple<int, int> Coords { get; }

            public TileState Attacked { get; set; }

            public Ship.Section Section { get; set; }
            
            public Mine Mine { get; set; }
        }

        // ############### BOARD.MINE ############### //
        public class Mine
        {
            public Mine() {
                Exploded = false;
            }
            
            public bool Exploded { get; set; }
        }
    }
}