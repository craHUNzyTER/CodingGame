using System;

/**
 * Help the Christmas elves fetch presents in a magical labyrinth!
 **/
namespace CodingGame.XmasRush
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputs;

            // game loop
            while (true)
            {
                int turnType = int.Parse(Console.ReadLine());
                for (int i = 0; i < 7; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    for (int j = 0; j < 7; j++)
                    {
                        string tile = inputs[j];
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int numPlayerCards = int.Parse(inputs[0]); // the total number of quests for a player (hidden and revealed)
                    int playerX = int.Parse(inputs[1]);
                    int playerY = int.Parse(inputs[2]);
                    string playerTile = inputs[3];
                }
                int numItems = int.Parse(Console.ReadLine()); // the total number of items available on board and on player tiles
                for (int i = 0; i < numItems; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string itemName = inputs[0];
                    int itemX = int.Parse(inputs[1]);
                    int itemY = int.Parse(inputs[2]);
                    int itemPlayerId = int.Parse(inputs[3]);
                }
                int numQuests = int.Parse(Console.ReadLine()); // the total number of revealed quests for both players
                for (int i = 0; i < numQuests; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string questItemName = inputs[0];
                    int questPlayerId = int.Parse(inputs[1]);
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Console.WriteLine("PUSH 3 RIGHT"); // PUSH <id> <direction> | MOVE <direction> | PASS
            }
        }
    }
}