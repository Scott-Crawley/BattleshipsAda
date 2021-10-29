﻿using System;
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

        public Tile[] Tiles { get; }

        public Board(Tuple<int, int> size, string name) {
            _boardName = name;
            _size = size;
            _totalSize = _size.Item1 * _size.Item2;
            if (_totalSize > 900) {                                                                                     // 900 = 30x30
                _warnSize = true;
            }
            Tiles = new Tile[_totalSize];
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
        
        private IEnumerable<Tile> GetTilesBetween(Tile startTile, Tile endTile, Orientation orientation) {
            if (startTile == null || endTile == null || orientation == Orientation.None) return null;
            
            var tiles = new List<Tile>();
            var (startX, startY) = startTile.Coords;
            var (endX, endY)     = endTile.Coords;

            var step = 1;
            if (orientation == Orientation.Horizontal) {
                if (startX > endX) {                                                                                    // endTile is to the left of startTile
                    step = -1;
                    for (var x = startX; endX <= x; x += step) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                    }
                }
                else {                                                                                                  // endTile is to the right of startTile
                    for (var x = startX; endX >= x; x += step) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                    }
                }
            }
            else {
                if (startY > endY) {                                                                                    // endTile is below startTile
                    step = -1;
                    for (var y = startY; endY <= y; y += step) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                    }
                }
                else {                                                                                                  // endTile is above startTile
                    for (var y = startY; endY >= y; y += step) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                    }
                }  
            }
            return tiles.ToArray();
        }
        
        
        // ############### PUBLIC METHODS ############### //
        public void Render(bool clear = false) {
            if (clear) Console.Clear();
            Console.WriteLine();
            Console.WriteLine(_boardName.PadLeft((_boardName.Length + (_size.Item1 * 3 / 4) + _size.Item1) / 2));       // Centre is calculated as this somehow ¯\_(ツ)_/¯
            
            var tileNo = 0;
            for (var row = _size.Item2; row > 0; row--) {                                                           // Decrement to get ascending Y-axis
                Console.Write($"{row,3}");
                
                for (var col = 0; col < _size.Item1; col++) {
                    var section  = Tiles[tileNo].Section;
                    
                    char shipChar;
                    ConsoleColor colour;
                    switch (Tiles[tileNo].Attacked) {
                        case TileState.Hit:
                            shipChar = 'X';
                            colour = ConsoleColor.Red;
                            break;
                        case TileState.Miss:
                            shipChar = '-';
                            colour = ConsoleColor.DarkCyan;
                            break;
                        default:
                            shipChar = '.';
                            colour = ConsoleColor.DarkGray;
                            break;
                    }
                    if (section != null) {                                                                              // Section always null on Target Boards; won't run (good!)
                        shipChar = section.Damaged ? 'X' : section.Ship.Name[0];                                        // If a section and not damaged, use the ship's initial
                        colour = section.Damaged ? ConsoleColor.Red : ConsoleColor.White;
                    }

                    Console.ForegroundColor = colour;
                    Console.Write($"{shipChar,3}");
                    Console.ResetColor();
                    tileNo++;
                }
                Console.WriteLine();
            }
            
            Console.WriteLine(GenerateXAxis());
            if (_warnSize) Console.WriteLine("WARN: Boards over 30x30 may introduce graphical issues\n");               // It [works] up to 701x701; [usability] not guaranteed
        }

        public Tile[] FindContinuousTilesAt(Tile startTile, Orientation orientation, int numTiles) {
            if (startTile == null)                     return null;
            if (orientation == Orientation.None)       return null;
            if (numTiles < 0 || numTiles > _totalSize) return null;

            var (x, y) = startTile.Coords;
            var endTileCoords = new Tuple<int, int>(0, 0);
            for (var i = 0; i < 4; i++) {
                if (orientation == Orientation.Vertical && (i == 0 || i == 1)) continue;                                // Don't calculate tiles for both orientations
                if (orientation == Orientation.Horizontal && i == 2) break;
                
                endTileCoords = i switch {                                                                              // -1 to numTiles because of zero-based array index
                    0 => new Tuple<int, int>(x + numTiles - 1, y),                                                      // EAST
                    1 => new Tuple<int, int>(x - numTiles - 1, y),                                                      // WEST
                    2 => new Tuple<int, int>(x, y + numTiles - 1),                                                      // NORTH
                    3 => new Tuple<int, int>(x, y - numTiles - 1),                                                      // SOUTH
                    _ => endTileCoords
                };
                var endTile = GetTileAt(endTileCoords);
                if (endTile?.Section != null) continue;                                                                 // Ensure endTile is free (and not null)
                
                var tiles = GetTilesBetween(startTile, endTile, orientation);
                if (tiles != null && tiles.All(tile => tile != null && tile.Section == null)) {                 // Ensure tiles in-between are free
                    return (Tile[]) tiles;
                }
            }
            return null;
        }

        public bool UnclaimTiles(Tile startTile, Tile endTile, Orientation orientation) {
            var tiles = GetTilesBetween(startTile, endTile, orientation);
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
            var tiles = Tiles;
            if (matchCriteria != null) {
                tiles = Tiles.Where(matchCriteria).ToArray();
            }
            return tiles.ElementAt(new Random().Next(0, tiles.Length - 1));
        }

        
        // ############### BOARD.TILE ############### //
        public class Tile
        {
            public Tile(Tuple<int,int> coords, Ship.Section shipSection = null) {
                Coords = coords;
                Section = shipSection;
                Attacked = TileState.None;
            }

            public Tuple<int, int> Coords { get; }

            public TileState Attacked { get; set; }

            public Ship.Section Section { get; set; }
        }
    }
}