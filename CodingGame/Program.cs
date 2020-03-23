using System;

// Write an action using Console.WriteLine()
// To debug: Console.Error.WriteLine("Debug messages...");
namespace CodingGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Input.ParseSizeOfMapAndMyId();

            Console.WriteLine("7 7");

            // game loop
            while (true)
            {
                Input.ParseTurn();

                Console.WriteLine("MOVE N TORPEDO");
            }
        }
    }

    #region Read input data

    public static class Input
    {
        private static string[] inputs;

        public static void ParseSizeOfMapAndMyId()
        {
            inputs = Console.ReadLine().Split(' ');

            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);

            int myId = int.Parse(inputs[2]);

            for (int i = 0; i < height; i++)
            {
                string line = Console.ReadLine();
            }
        }

        public static void ParseTurn()
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int myLife = int.Parse(inputs[2]);
            int oppLife = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = Console.ReadLine();
            string opponentOrders = Console.ReadLine();
        }
    }

    #endregion Read input data
}