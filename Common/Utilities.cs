using System;
using System.Collections.Generic;

namespace BattleshipsAda
{
    public class Utilities
    {
        public static string RequestInput(string prompt) {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public static void OutputList(IEnumerable<string> items) {
            var i = 1;
            foreach (var item in items) {
                Console.WriteLine($"{i}: {item}");
                i++;
            }
        }

        public static int RequestChoice(int maxValue) {
            while (true) {
                var input = RequestInput("Selection: ");
                if (!int.TryParse(input, out var choice)) continue;
                if (choice <= maxValue && choice >= 0) return choice;
            }
        }
    }
}