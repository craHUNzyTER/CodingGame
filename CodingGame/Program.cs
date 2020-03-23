using System;

// Write an action using Console.WriteLine()
// To debug: Console.Error.WriteLine("Debug messages...");
namespace CodingGame
{
    class Program
    {
        static void Main(string[] args)
        {
            GameData.InitializeGame();

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

    public static class GameData
    {
        public static void InitializeGame()
        {
            Map = new Cell[Constants.MapWidth, Constants.MapHeight];
        }

        public static Cell[,] Map { get; set; }
    }

    public class Coordinate
    {
        public int X { get; }
        public int Y { get; }

        #region Constructors and overriden methods

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}";
        }

        public override bool Equals(object obj)
        {
            Coordinate other = obj as Coordinate;

            if (ReferenceEquals(null, obj))
                return false;

            if (X == other.X && Y == other.Y)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return 7333 ^ X.GetHashCode() ^ Y.GetHashCode();
        }

        #endregion Constructors and overriden methods
    }

    public class Cell
    {
        public Coordinate Coordinate { get; }
        public bool IsIsland { get; }

        #region Constructors and overriden methods

        public Cell(Coordinate coordinate, bool isIsland)
        {
            Coordinate = coordinate;
            IsIsland = isIsland;
        }

        public override string ToString()
        {
            return $"{Coordinate}, IsIsland={IsIsland}";
        }

        #endregion Constructors and overriden methods     
    }

    static class Constants
    {
        public static int MapWidth = 15;
        public static int MapHeight = 15;
    }

    #region Read input data

    public static class Input
    {
        private static string[] _inputs;

        public static void ParseSizeOfMapAndMyId()
        {
            _inputs = Console.ReadLine().Split(' ');

            int myId = int.Parse(_inputs[2]);

            ParseMap();
        }

        public static void ParseTurn()
        {
            _inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(_inputs[0]);
            int y = int.Parse(_inputs[1]);
            int myLife = int.Parse(_inputs[2]);
            int oppLife = int.Parse(_inputs[3]);
            int torpedoCooldown = int.Parse(_inputs[4]);
            int sonarCooldown = int.Parse(_inputs[5]);
            int silenceCooldown = int.Parse(_inputs[6]);
            int mineCooldown = int.Parse(_inputs[7]);
            string sonarResult = Console.ReadLine();
            string opponentOrders = Console.ReadLine();
        }

        private static void ParseMap()
        {
            for (int y = 0; y < Constants.MapHeight; y++)
            {
                string line = Console.ReadLine();

                for (int x = 0; x < Constants.MapWidth; x++)
                {
                    var isIsland = line[x] == 'x';

                    GameData.Map[y, x] = new Cell(new Coordinate(x, y), isIsland);
                }
            }

            for (int x = 0; x < 1; x++)
            {
                for (int y = 0; y < 1; y++)
                {
                    Console.Error.WriteLine($"Parsed cell: {GameData.Map[x, y]}");
                }
            }
        }
    }

    #endregion Read input data
}