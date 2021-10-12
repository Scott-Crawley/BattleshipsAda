using System;
using BattleshipsAda.Common;

namespace BattleshipsAda.Board
{
    public class Board
    {
        private readonly Tuple<int, int> _size = new Tuple<int, int>(10, 10);
        private readonly Team _team;

        public Tile[] Tiles { get; }

        public Board(Team team, Tuple<int,int> size) {
            _size ??= size;
            _team = team;

            var totalSize = _size.Item1 * _size.Item2;
            Tiles = new Tile[totalSize];
        }
        
        public class Tile
        {
            public Tile(string coords, Ship.Section shipSection = null) {
                Coords = coords;
                Section = shipSection;
                Attacked = TileState.NONE;
            }

            public string Coords { get; }

            public TileState Attacked { get; set; }

            public Ship.Section Section { get; set; }
        }
    }
}