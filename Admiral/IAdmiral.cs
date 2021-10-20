namespace BattleshipsAda
{
    public interface IAdmiral
    {
        public Board Board { get; }
        public Board TargetBoard { get; }

        public abstract void AttackTile(Board.Tile tile);

        public abstract Board.Tile GetTileAsInput();

        public abstract void AutoPlaceShips(bool allShips = true);

        public abstract void DoTurn();

        public abstract bool IsDefeated();

        public abstract void SetupFleet();
    }
}