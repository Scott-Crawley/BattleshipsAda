using System;

namespace BattleshipsAda
{
    public class Ship
    {
        private readonly ShipInfo _info;

        private int _damageValue;

        public bool Destroyed { get; private set; }
        public Direction Direction { get; set; }
        public int Length => _info.Length;
        public string Name => _info.Name;
        public Section[] Sections { get; }
        public bool Placed { get; set; }
        public Board.Tile StartTile { get; set; }
        public Board.Tile EndTile { get; set; }

        public delegate void DestroyedStatusHandler(object sender, EventArgs e);
        public event DestroyedStatusHandler OnDestroyedEvent;                                                           // See 'Fleet' constructor

        public Ship(ShipInfo info, Direction direction = Direction.None) {
            _info = info;
            Direction = direction;
            Sections = new Section[Length];
            PopulateSections();
        }

        private void PopulateSections() {
            for (var i = 0; i < Sections.Length; i++) {
                Sections[i] = new Section(this);
            }
        }

        private void UpdateDamage() {
            if (Destroyed) return;
            if (_damageValue + 1 >= Length) {
                Destroyed = true;
                OnDestroyedEvent?.Invoke(this, EventArgs.Empty);
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