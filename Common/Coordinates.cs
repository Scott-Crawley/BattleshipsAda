using System;

namespace BattleshipsAda
{
    public class Coordinates
    {
        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        public static Tuple<int, int> GetNextCoord(int maxX, Tuple<int, int> lastCoord) {
            int x, y;
            if (lastCoord.Item1 + 1 <= maxX) {                                                                          // Wrap around once we've maxed out X's value
                x = lastCoord.Item1 + 1;                                                                                // X axis is already ascending, so unchanged
                y = lastCoord.Item2;
            }
            else {
                x = 0;                                                                                                  // Reset X to 0 and decrement Y
                y = lastCoord.Item2 - 1;
            }
            return new Tuple<int, int>(x, y);
        }
        
        public static bool CoordsValid(Tuple<int, int> boardSize, Tuple<int, int> coords) {
            var (x, y) = coords;
            return y < boardSize.Item2 && y >= 0 && x < boardSize.Item1 && x >= 0;                                      // Ensure x/y are in the board's range
        }

        public static int GetLetterCoordValue(string coord) {
            coord = coord.ToUpper();
            var first = ALPHABET.IndexOf(coord[0]);
            switch (coord.Length) {
                case 1:
                    return first;
                case 2: 
                    var wraps = (first + 1) * 26;
                    return wraps + ALPHABET.IndexOf(coord[1]);
                default:
                    return -1;
            }
        }
    }
}