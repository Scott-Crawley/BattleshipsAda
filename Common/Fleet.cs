using System.Linq;

namespace BattleshipsAda
{
    public class Fleet
    {
        private Admiral _admiral;
        
        public Ship[] Ships { get; }

        public Fleet(Admiral admiral) {
            _admiral = admiral;
            Ships = Controller.Get().ShipTypes.Select(type => new Ship(type)).ToArray();
        }

        public Ship[] GetUnplacedShips() {
            return Ships.Where(ship => !ship.Placed).ToArray();
        }
        
        public Ship[] GetPlacedShips() {
            return Ships.Where(ship => ship.Placed).ToArray();
        }

        public bool PlaceShip(Ship ship, Board.Tile startTile, Orientation orientation) {
            if (!Ships.Contains(ship)) return false;
            if (!_admiral.Board.Tiles.Contains(startTile)) return false;
            if (startTile.Section.Ship != null && startTile.Section.Ship != ship) return false;

            if (ship.Placed) {
                if (!UnplaceShip(ship)) return false;
            }

            var tiles = _admiral.Board.ReserveTiles(startTile, orientation, ship.Length);
            if (tiles == null) return false;
            
            for (var i = 0; i < ship.Length; i++) {
                if (tiles[i] == null) return false;
                tiles[i].Section = ship.Sections[i];
            }
            
            ship.Placed = true;
            ship.StartTile = startTile;
            ship.EndTile = tiles[ship.Length - 1];
            return true;
        }

        public bool UnplaceShip(Ship ship) {
            if (!Ships.Contains(ship)) return false;
            if (!ship.Placed) return true;

            if (!_admiral.Board.FreeTiles(ship.StartTile, ship.EndTile, ship.Orientation)) return false;
            ship.Placed = false;
            return true;
        }
    }
}