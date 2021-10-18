using System;

namespace BattleshipsAda
{
    public class Ship
    {
        private readonly ShipInfo _info;

        private int _damageValue;
        private bool _destroyed;

        public Orientation Orientation { get; set; }
        public int Length => _info.Length;
        public string Name => _info.Name;
        public Section[] Sections { get; }
        public bool Placed { get; set; }
        public Board.Tile StartTile { get; set; }
        public Board.Tile EndTile { get; set; }

        public delegate void DestroyedStatusHandler(object sender, EventArgs e);
        public event DestroyedStatusHandler OnDestroyedEvent;

        public Ship(ShipInfo info, Orientation orientation = Orientation.NONE) {
            _info = info;
            Orientation = orientation;
            Sections = new Section[Length];
            PopulateSections();
        }

        private void PopulateSections() {
            for (var i = 0; i < Sections.Length; i++) {
                Sections[i] = new Section(this);
            }
        }

        private void UpdateDamage() {
            if (_destroyed) return;
            if (_damageValue + 1 >= Length) {
                _destroyed = true;
                OnDestroyedEvent(this, EventArgs.Empty);
            }
            else {
                _damageValue++;
            }
        }

        public class Section
        {
            private bool _damaged;
            
            public bool Damaged {
                get => _damaged;
                set {
                    if (_damaged || value == false) return;
                    Ship.UpdateDamage();
                    _damaged = true;
                }
            }

            public Ship Ship { get; }
            
            public Section(Ship ship) {
                Ship = ship;
            }
        }
    }
}