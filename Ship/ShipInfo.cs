namespace BattleshipsAda
{
    public class ShipInfo
    {
        public readonly string Name;
        public readonly int Length;

        public ShipInfo(string name, int length) {
            // Make first letter of name capitalised
            name = name[0].ToString().ToUpper() + name.Substring(1);                                                    // We make it lowercase in Config.cs to standardise inputs
            Name = name;
            Length = length;
        }
    }
}