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

            var startLocation = new Coordinate(7, 7);
            Output.PrintStartLocation(startLocation);

            // game loop
            while (true)
            {
                GameData.InitializeTurn();

                Input.ParseTurn();

                Game.Play();

                Output.PrintCommand();
            }
        }
    }

    public static class Game
    {
        public static void Play()
        {
            Output.AppendMove(Direction.N, System.TORPEDO);
        }
    }

    public static class GameData
    {
        public static void InitializeGame()
        {
            Map = new Cell[Constants.MapWidth, Constants.MapHeight];
        }

        public static void InitializeTurn()
        {
            OutputCommand = default;
        }

        public static Cell[,] Map { get; set; }

        public static int MyLife { get; set; }
        public static int OpponentLife { get; set; }

        public static int TorpedoCooldown { get; set; }
        public static int SonarCooldown { get; set; }
        public static int SilenceCooldown { get; set; }
        public static int MineCooldown { get; set; }

        public static string OutputCommand { get; set; }
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

    public enum Direction
    {
        N,
        W,
        S,
        E
    }

    public enum System
    {
        TORPEDO,
        SONAR,
        SILENCE,
        MINE
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

            int myCurrentX = int.Parse(_inputs[0]);
            int myCurrentY = int.Parse(_inputs[1]);

            GameData.MyLife = int.Parse(_inputs[2]);
            GameData.OpponentLife = int.Parse(_inputs[3]);
            GameData.TorpedoCooldown = int.Parse(_inputs[4]);
            GameData.SonarCooldown = int.Parse(_inputs[5]);
            GameData.SilenceCooldown = int.Parse(_inputs[6]);
            GameData.MineCooldown = int.Parse(_inputs[7]);

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

    public static class Output
    {
        public static void AppendMove(Direction direction, System systemToCharge)
        {
            var command = $"MOVE {direction} {systemToCharge}";
            AppendCommand(command);
        }

        public static void PrintStartLocation(Coordinate coordinate)
        {
            Console.WriteLine($"{coordinate.X} {coordinate.Y}");
        }

        public static void PrintCommand()
        {
            Console.WriteLine(GameData.OutputCommand);
        }

        private static void AppendCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(GameData.OutputCommand))
            {
                GameData.OutputCommand = command;
            }
            else
            {
                GameData.OutputCommand += "|" + command;
            }
        }
    }
}