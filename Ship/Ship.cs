using System;

namespace BattleshipsAda
{
    public class Ship
    {
        private readonly ShipType _type;

        private bool _destroyed;

        public Orientation Orientation { get; }
        public int Length { get; }
        public Section[] Sections { get; }
        public bool Placed { get; set; }
        public Board.Tile StartTile { get; set; }
        public Board.Tile EndTile { get; set; }

        public Ship(ShipType type, Orientation orientation = Orientation.NONE) {
            _type = type;
            Orientation = orientation;
            _destroyed = false;
            Length = type.Length;
            Sections = new Section[Length];
            PopulateSections();
        }

        private void PopulateSections() {
            for (var i = 0; i < Sections.Length; i++) {
                Sections[i] = new Section(this);
            }
        }

        public class Section
        {
            public bool Damaged { get; set; }
            public Ship Ship { get; }
            
            public Section(Ship ship) {
                Ship = ship;
                Damaged = false;
            }
        }
    }
}