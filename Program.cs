using System;

namespace BattleshipsAda
{
    class Program
    {
        static void Main(string[] args) {
            var controller = Controller.Get();
            if (controller.Setup) {
                Utilities.RequestInput("Press any key to play...");
                controller.Play();
            }
            Console.WriteLine("Exiting...");
        }
    }
}