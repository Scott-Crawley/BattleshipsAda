using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipsAda
{
    public class Board
    {
        private readonly Tuple<int, int> _size = new Tuple<int, int>(10, 10);
        private readonly int _totalSize;

        public Tile[] Tiles { get; }

        public Board(Tuple<int, int> size) {
            _size ??= size;
            _totalSize = _size.Item1 * _size.Item2;
            Tiles = new Tile[_totalSize];
        }

        public Tile[] ReserveTiles(Tile startTile, Orientation orientation, int numTiles) {
            if (startTile == null) return null;
            if (orientation == Orientation.NONE) return null;
            if (numTiles < 0 || numTiles > _totalSize) return null;

            var horizontal = orientation == Orientation.HORIZONTAL;
            var (x, y) = startTile.Coords;

            IEnumerable<Tile> tiles;
            if (horizontal) {
                var endTileEast = GetTileAt(new Tuple<int, int>(x + numTiles, y));
                tiles = GetTilesBetween(startTile, endTileEast, Orientation.HORIZONTAL);
                if (ValidateTiles(tiles)) {
                    return (Tile[]) tiles;
                }
                
                var endTileWest = GetTileAt(new Tuple<int, int>(x - numTiles, y));
                tiles = GetTilesBetween(startTile, endTileWest, Orientation.HORIZONTAL);
                if (ValidateTiles(tiles)) {
                    return (Tile[]) tiles;
                }
            }
            else {
                var endTileNorth = GetTileAt(new Tuple<int, int>(x, y + numTiles));
                tiles = GetTilesBetween(startTile, endTileNorth, Orientation.VERTICAL);
                if (ValidateTiles(tiles)) {
                    return (Tile[]) tiles;
                }
                
                var endTileSouth = GetTileAt(new Tuple<int, int>(x, y - numTiles));
                tiles = GetTilesBetween(startTile, endTileSouth, Orientation.VERTICAL);
                if (ValidateTiles(tiles)) {
                    return (Tile[]) tiles;
                }
            }
            return null;
        }

        public bool FreeTiles(Tile startTile, Tile endTile, Orientation orientation) {
            var tiles = GetTilesBetween(startTile, endTile, orientation);
            var tileCount = 0;
            
            foreach (var tile in tiles) {
                tileCount++;
                tile.Section = null;
            }
            return tileCount >= 1;
        }

        public Tile GetTileAt(Tuple<int, int> coords) {
            return Tiles.First(tile => Equals(tile.Coords, coords));
        }

        public IEnumerable<Tile> GetTilesBetween(Tile startTile, Tile endTile, Orientation orientation) {
            var tiles = new List<Tile>();
            
            var startX = startTile.Coords.Item1;
            var startY = startTile.Coords.Item2;
            var endX = endTile.Coords.Item1;
            var endY = endTile.Coords.Item2;
            
            if (orientation == Orientation.HORIZONTAL) {
                if (startX > endX) {                                                                                    // endTile is to the left of startTile
                    for (var x = startX; endX < x; x--) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                    }
                }
                else {                                                                                                  // endTile is to the right of startTile
                    for (var x = startX; endX > x; x++) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                    }
                }
            }
            else {
                if (startY > endY) {                                                                                    // endTile is below startTile
                    for (var y = startY; endY < y; y--) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                    }
                }
                else {                                                                                                  // endTile is above startTile
                    for (var y = startY; endY > y; y++) {
                        tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                    }
                }
            }
            return tiles.ToArray();
        }

        public static bool ValidateTiles(IEnumerable<Tile> tiles) {
            return tiles.All(tile => tile != null && tile.Section == null);
        }

        public class Tile
        {
            public Tile(Tuple<int,int> coords, Ship.Section shipSection = null) {
                Coords = coords;
                Section = shipSection;
                Attacked = TileState.NONE;
            }

            public Tuple<int, int> Coords { get; }

            public TileState Attacked { get; set; }

            public Ship.Section Section { get; set; }
        }
    }
}