namespace BattleshipsAda
{
    internal static class Program
    {
        private static void Main() {
            var controller = Controller.Get();
            if (controller.Setup) {
                Utilities.RequestInput("Press any key to play...");
                controller.Play();
            }

            Utilities.RequestInput("\nPress any key to quit...");
        }
    }
}