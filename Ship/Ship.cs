namespace BattleshipsAda
{
    public class Ship
    {
        private readonly string _type;
        private readonly int _length;
        private readonly Section[] _sections;

        private Orientation _orientation;
        private bool _destroyed;

        public Ship(string type, int length, Orientation orientation = Orientation.NONE) {
            _type = type;
            _length = length;
            _orientation = orientation;
            _destroyed = false;
            _sections = new Section[length];
        }
        
        public class Section
        {
            public Section() {
                Damaged = false;
            }

            public bool Damaged { get; set; }
        }
    }
}