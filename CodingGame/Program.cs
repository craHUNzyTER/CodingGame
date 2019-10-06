using System;
using System.Collections.Generic;
using System.Linq;

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
            GameData.InitializeGame();

            Input.ParseSizeOfMap();

            // game loop
            while (true)
            {
                GameData.InitializeTurn();

                Input.ParseTurn();

                Game.Play();

                Output.Print();
            }
        }
    }

    static class Game
    {
        public static void Play()
        {
            DeliverOre();

            BuryRadar();

            RequestRadar();

            //RequestTrap();

            DigVeinCells();

            DigUnknownCells();

            DefaultMove();
        }

        private static void DeliverOre()
        {
            foreach (var robot in Helpers.GetRobotsWithOre())
            {
                GameData.OutputCommands[robot.Id] = Output.Move(Constants.HeadquartersX, robot.Coordinate.Y);
            }
        }

        private static void RequestRadar()
        {
            var robotInHq = Helpers.GetRobotInHeadquarters();
            if (GameData.RadarCooldown == 0 && robotInHq != null)
            {
                GameData.OutputCommands[robotInHq.Id] = Output.RequestRadar();
            }
        }

        private static void RequestTrap()
        {
            var robotInHq = Helpers.GetRobotInHeadquarters();
            if (GameData.TrapCooldown == 0 && robotInHq != null)
            {
                GameData.OutputCommands[robotInHq.Id] = Output.RequestTrap();
            }
        }

        private static void BuryRadar()
        {
            foreach (var robot in Helpers.GetRobotsWithRadar())
            {
                GameData.OutputCommands[robot.Id] = Output.Dig(10, robot.Coordinate.Y);
            }
        }

        private static void DigVeinCells()
        {
            if (GameData.VeinCells.Count == 0)
                return;

            var notAssignedRobots = Helpers.GetNotAssignedRobots();
            for (int i = 0; i < notAssignedRobots.Count; i++)
            {
                if (GameData.VeinCells.Count > i)
                {
                    var coordinate = GameData.VeinCells[i].Coordinate;
                    GameData.OutputCommands[notAssignedRobots[i].Id] = Output.Dig(coordinate.X, coordinate.Y);
                }
            }
        }

        private static void DigUnknownCells()
        {

        }

        private static void DefaultMove()
        {
            foreach (var robot in Helpers.GetNotAssignedRobots())
            {
                GameData.OutputCommands[robot.Id] = Output.Wait();
            }
        }
    }

    static class Helpers
    {
        public static List<Robot> GetNotAssignedRobots()
        {
            var notAssignedRobots = GameData.MyRobots
                .Where(r => !Output.HasAssignedCommand(r.Id))
                .ToList();

            Console.Error.WriteLine($"Not assigned robots: {notAssignedRobots.Count}.");

            return notAssignedRobots;
        }

        public static List<Robot> GetRobotsWithOre()
        {
            return GetNotAssignedRobots()
                .Where(r => r.CarryOre)
                .ToList();
        }

        public static List<Robot> GetRobotsWithRadar()
        {
            return GetNotAssignedRobots()
                .Where(r => r.CarryRadar)
                .ToList();
        }

        public static Robot GetRobotInHeadquarters()
        {
            return GetNotAssignedRobots()
                .Where(r => r.Coordinate.X == Constants.HeadquartersX)
                .FirstOrDefault();
        }
    }

    static class GameData
    {
        public static void InitializeGame()
        {
            Map = new Cell[Constants.MapHeight, Constants.MapWidth];
        }

        public static void InitializeTurn()
        {
            Cells = new List<Cell>(Constants.MapHeight * Constants.MapWidth);

            MyRobots = new List<Robot>(Constants.OneTeamRobotsCount);
            OpponentRobots = new List<Robot>(Constants.OneTeamRobotsCount);

            OutputCommands = new string[Constants.OneTeamRobotsCount];
        }

        public static int RadarCooldown { get; set; }
        public static int TrapCooldown { get; set; }

        public static Cell[,] Map { get; set; }
        public static List<Cell> Cells { get; set; }
        public static List<Cell> VeinCells { get; set; }

        public static List<Robot> MyRobots { get; set; }
        public static List<Robot> OpponentRobots { get; set; }

        public static string[] OutputCommands { get; set; } // output for every turn
    }

    class Coordinate
    {
        public int X { get; private set; }
        public int Y { get; private set; }

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

    class Robot
    {
        private EntityType _type;

        public int Id { get; private set; }
        public Coordinate Coordinate { get; private set; }
        public EntityType Item { get; private set; }

        #region Constructors and overriden methods

        public Robot(int id, EntityType type, Coordinate coordinate, EntityType item)
        {
            Id = id;
            _type = type;
            Coordinate = coordinate;
            Item = item;
        }

        public override string ToString()
        {
            var owner = IsMine ? "My" : "Op";
            return $"{owner}Robot. Id={Id}. {Coordinate}. Item={Item}";
        }

        public override bool Equals(object obj)
        {
            Robot other = obj as Robot;

            if (ReferenceEquals(null, obj))
                return false;

            if (Id == other.Id)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return 7333 ^ Id.GetHashCode();
        }

        #endregion Constructors and overriden methods

        public bool IsMine => _type == EntityType.MyRobot;

        public bool CarryOre => Item == EntityType.Ore;

        public bool CarryRadar => Item == EntityType.Radar;
    }

    class Cell
    {
        public Coordinate Coordinate { get; private set; }
        public int OreCount { get; private set; }
        public bool HasHole { get; private set; }

        #region Constructors and overriden methods

        public Cell(Coordinate coordinate, int oreCount, bool hasHole)
        {
            Coordinate = coordinate;
            OreCount = oreCount;
            HasHole = hasHole;
        }

        public override string ToString()
        {
            return $"{Coordinate}, Ore={OreCount}, Hole={HasHole}";
        }

        #endregion Constructors and overriden methods

        public bool HasOre => OreCount > 0;
    }

    enum EntityType
    {
        None = -1,
        MyRobot = 0,
        OpponentRobot = 1,
        Radar = 2,
        Trap = 3,
        Ore = 4

        // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
        // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
    }

    static class Constants
    {
        public static int MapWidth = 30;
        public static int MapHeight = 15;
        public static int HeadquartersX = 0;

        public static int OneTeamRobotsCount = 5;
    }

    #region Read input data

    static class Input
    {
        private static string[] _inputs;

        public static void ParseSizeOfMap()
        {
            Console.ReadLine();
        }

        public static void ParseTurn()
        {
            _inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(_inputs[0]); // Amount of ore delivered
            int opponentScore = int.Parse(_inputs[1]);

            ParseMap();

            ParseEntities();
        }

        private static void ParseMap()
        {
            for (int y = 0; y < Constants.MapHeight; y++)
            {
                _inputs = Console.ReadLine().Split(' ');
                for (int x = 0; x < Constants.MapWidth; x++)
                {
                    string ore = _inputs[2 * x];// amount of ore or "?" if unknown
                    int oreCount = ore == "?" ? -1 : int.Parse(ore);

                    int hole = int.Parse(_inputs[2 * x + 1]);// 1 if cell has a hole
                    bool hasHole = hole == 1;

                    var cell = new Cell(new Coordinate(x, y), oreCount, hasHole);
                    GameData.Map[y, x] = cell;
                    GameData.Cells.Add(cell);
                }
            }

            GameData.VeinCells = GameData.Cells.Where(c => c.HasOre).ToList();
            Console.Error.WriteLine($"Vein cells count: {GameData.VeinCells.Count}");

            for (int x = 0; x < 1; x++)
            {
                for (int y = 0; y < 1; y++)
                {
                    //Console.Error.WriteLine($"Parsed cell: {GameData.Map[x, y]}");
                }
            }
        }

        private static void ParseEntities()
        {
            _inputs = Console.ReadLine().Split(' ');
            int entityCount = int.Parse(_inputs[0]); // number of entities visible to you
            GameData.RadarCooldown = int.Parse(_inputs[1]); // turns left until a new radar can be requested
            GameData.TrapCooldown = int.Parse(_inputs[2]); // turns left until a new trap can be requested
            for (int i = 0; i < entityCount; i++)
            {
                _inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(_inputs[0]); // unique id of the entity
                EntityType type = (EntityType)int.Parse(_inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap

                int x = int.Parse(_inputs[2]);
                int y = int.Parse(_inputs[3]); // position of the entity
                var coordinate = new Coordinate(x, y);

                EntityType item = (EntityType)int.Parse(_inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)

                if (type == EntityType.MyRobot || type == EntityType.OpponentRobot)
                {
                    var robot = new Robot(id, type, coordinate, item);

                    if (robot.IsMine)
                        GameData.MyRobots.Add(robot);
                    else
                        GameData.OpponentRobots.Add(robot);

                    //Console.Error.WriteLine($"Parsed robot: {robot}");
                }
            }
        }
    }

    #endregion Read input data

    // WAIT|MOVE x y|DIG x y|REQUEST item
    static class Output
    {
        public static string Wait()
        {
            return "WAIT";
        }

        public static string RequestRadar()
        {
            return Request("RADAR");
        }

        public static string RequestTrap()
        {
            return Request("TRAP");
        }

        private static string Request(string item)
        {
            return "REQUEST " + item;
        }

        public static string Move(int x, int y)
        {
            return $"MOVE {x} {y}";
        }

        public static string Dig(int x, int y)
        {
            return $"DIG {x} {y}";
        }

        public static bool HasAssignedCommand(int robotId)
        {
            return !string.IsNullOrEmpty(GameData.OutputCommands[robotId]);
        }

        public static void Print()
        {
            foreach (var command in GameData.OutputCommands)
            {
                Console.WriteLine(command);
            }
        }
    }
}