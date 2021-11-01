using System;
using System.Linq;

namespace BattleshipsAda
{
    public class Fleet
    {
        private readonly IAdmiral _admiral;
        
        public Ship[] Ships { get; }
        public int DestroyedShips { get; private set; }

        public Fleet(IAdmiral admiral) {
            _admiral = admiral;
            Ships = Controller.Get().ShipTypes.Select(type => {
                var ship = new Ship(type);
                ship.OnDestroyedEvent += UpdateDestroyed;                                                               // Event delegate modifies Fleet without cyclical dependency
                return ship;
            }).ToArray();
        }

        public Ship[] GetUnplacedShips() {
            return Ships.Where(ship => !ship.Placed).ToArray();
        }
        
        public Ship[] GetPlacedShips() {
            return Ships.Where(ship => ship.Placed).ToArray();
        }

        private void UpdateDestroyed(object sender, EventArgs e) {
            DestroyedShips++;
        }

        public bool PlaceShip(Ship ship, Board.Tile startTile, Direction direction) {
            if (!Ships.Contains(ship)) return false;                                                                    // Ensure we own this ship
            if (!_admiral.Board.Tiles.Contains(startTile)) return false;                                                // Ensure we own this tile
            if (startTile.Section != null && startTile.Section.Ship != ship) return false;                              // Ensure the tile is free (excluding if it's this ship)

            if (ship.Placed) {
                if (!UnplaceShip(ship)) return false;                                                                   // If our ship is already placed, we can move it by unplacing
            }

            var tiles = _admiral.Board.FindContinuousTilesAt(startTile, direction, ship.Length);
            if (tiles == null) return false;
            
            for (var i = 0; i < ship.Length; i++) {
                if (tiles[i] == null) return false;
                tiles[i].Section = ship.Sections[i];
            }
            
            ship.Placed = true;
            ship.Direction = direction;
            ship.StartTile = startTile;
            ship.EndTile = tiles[ship.Length - 1];
            return true;
        }

        public bool UnplaceShip(Ship ship) {
            if (!Ships.Contains(ship)) return false;
            if (!ship.Placed) return true;

            if (!_admiral.Board.UnclaimTiles(ship.StartTile, ship.EndTile, ship.Direction)) return false;
            ship.Placed = false;
            ship.Direction = Direction.None;
            ship.StartTile = null;
            ship.EndTile = null;
            return true;
        }
    }
}