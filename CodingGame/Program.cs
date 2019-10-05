﻿using System;

/**
* Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
**/

// Write an action using Console.WriteLine()
// To debug: Console.Error.WriteLine("Debug messages...");

namespace CodingGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Input.ParseSizeOfMap();

            // game loop
            while (true)
            {
                Input.ParseTurn();

                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("WAIT"); // WAIT|MOVE x y|DIG x y|REQUEST item
                }
            }
        }
    }

    #region Read input data

    static class Input
    {
        private static string[] inputs;
        private static int width;
        private static int height;

        public static void ParseSizeOfMap()
        {
            inputs = Console.ReadLine().Split(' ');
            width = int.Parse(inputs[0]);
            height = int.Parse(inputs[1]); // size of the map
        }

        public static void ParseTurn()
        {
            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]); // Amount of ore delivered
            int opponentScore = int.Parse(inputs[1]);
            for (int i = 0; i < height; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                for (int j = 0; j < width; j++)
                {
                    string ore = inputs[2 * j];// amount of ore or "?" if unknown
                    int hole = int.Parse(inputs[2 * j + 1]);// 1 if cell has a hole
                }
            }
            inputs = Console.ReadLine().Split(' ');
            int entityCount = int.Parse(inputs[0]); // number of entities visible to you
            int radarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
            int trapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // unique id of the entity
                int type = int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]); // position of the entity
                int item = int.Parse(inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
            }
        }
    }

    #endregion Read input data
}