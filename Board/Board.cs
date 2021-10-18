using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipsAda
{
    public class Board
    {
        private static readonly string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly Tuple<int, int> _size;
        private readonly int _totalSize;

        public Tile[] Tiles { get; }

        public Board(Tuple<int, int> size) {
            _size = size;
            _totalSize = _size.Item1 * _size.Item2;
            Tiles = new Tile[_totalSize];
            PopulateTiles();
        }

        private void PopulateTiles() {
            var coord = new Tuple<int, int>(0, 0);
            for (var i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new Tile(coord);
                coord = GetNextCoord(coord);
            }
        }

        private Tuple<int, int> GetNextCoord(Tuple<int, int> lastCoord) {
            int x, y;
            if (lastCoord.Item1 + 1 > _size.Item1 - 1) {
                x = 0;
                y = lastCoord.Item2 + 1;
            }
            else {
                x = lastCoord.Item1 + 1;
                y = lastCoord.Item2;
            }
            return new Tuple<int, int>(x, y);
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
                if (wraps >= 0) axisStr += Alphabet[wraps];
                axisStr += Alphabet[index] + "  ";
                total++; 
                index++;
            }
            return axisStr;
        }
        
        public async void Render() {
            var tileNo = 0;
            for (var row = 0; row < _size.Item2; row++) {
                var rowStr = $"{row,3}  ";
                for (var column = 0; column < _size.Item1; column++) {
                    rowStr += Tiles[tileNo].Section == null ? "." : $"{Tiles[tileNo].Section.Ship.Name[0]}";
                    rowStr += "  ";
                    tileNo++;
                }
                Console.WriteLine(rowStr);
            }
            Console.WriteLine(GenerateXAxis());
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
            return Tiles.FirstOrDefault(tile => Equals(tile.Coords, coords));
        }

        public IEnumerable<Tile> GetTilesBetween(Tile startTile, Tile endTile, Orientation orientation) {
            if (startTile == null || endTile == null || orientation == Orientation.NONE) return null;
            
            var tiles = new List<Tile>();
            var (startX, startY) = startTile.Coords;
            var (endX, endY)     = endTile.Coords;

            var step = 1;
            if (startX > endX) {                                                                                        // endTile is to the left of startTile
                step = -1;
                for (var x = startX; endX <= x; x += step) {
                    tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                }
            }
            else {                                                                                                      // endTile is to the right of startTile
                for (var x = startX; endX >= x; x += step) {
                    tiles.Add(GetTileAt(new Tuple<int, int>(x, startTile.Coords.Item2)));
                }
            }
            if (startY > endY) {                                                                                        // endTile is below startTile
                step = -1;
                for (var y = startY; endY <= y; y += step) {
                    tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                }
            }
            else {                                                                                                      // endTile is above startTile
                for (var y = startY; endY >= y; y += step) {
                    tiles.Add(GetTileAt(new Tuple<int, int>(startTile.Coords.Item1, y)));
                }
            }
            return tiles.ToArray();
        }

        public static bool ValidateTiles(IEnumerable<Tile> tiles) {
            return tiles != null && tiles.All(tile => tile != null && tile.Section == null);
        }

        public Tile GetRandomFreeTile() {
            var freeTiles = Tiles.Where(tile => tile.Section == null).ToArray();
            return freeTiles.ElementAt(new Random().Next(0, freeTiles.Length - 1));
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