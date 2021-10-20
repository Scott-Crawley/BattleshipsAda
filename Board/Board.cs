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

        public Tile[] Tiles { get; }

        public Board(Tuple<int, int> size, string name) {
            _boardName = name;
            _size = size;
            _totalSize = _size.Item1 * _size.Item2;
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
                if (index >= 26) {
                    index = 0;
                    wraps++;
                }
                if (wraps >= 0) axisStr += Coordinates.ALPHABET[wraps];                                                 // Wrapping allows for theoretical max size of 676x676 (ZZ)
                axisStr += Coordinates.ALPHABET[index] + "  ";
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
        public void Render() {
            Console.WriteLine();
            Console.WriteLine(_boardName.PadLeft(_size.Item1 * 2));                                                     // Centre is (very roughly) twice the X-axis length
            
            var tileNo = 0;
            for (var row = _size.Item2 - 1; row >= 0; row--) {                                                      // Decrement to get ascending Y-axis
                var rowStr = $"{row,3}  ";
                for (var column = 0; column < _size.Item1; column++) {
                    rowStr += Tiles[tileNo].Section == null ? "." : $"{Tiles[tileNo].Section.Ship.Name[0]}";
                    rowStr += "  ";
                    tileNo++;
                }
                Console.WriteLine(rowStr);
            }
            Console.WriteLine(GenerateXAxis());
            Console.WriteLine();
        }

        public Tile[] FindContinuousTilesAt(Tile startTile, Orientation orientation, int numTiles) {
            if (startTile == null) return null;
            if (orientation == Orientation.None) return null;
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
                if (tiles != null && tiles.All(tile => tile != null && tile.Section == null)) {                 // Ensure tiles in between are free
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

        public Tile GetRandomFreeTile() {
            var freeTiles = Tiles.Where(tile => tile.Section == null).ToArray();
            return freeTiles.ElementAt(new Random().Next(0, freeTiles.Length - 1));
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