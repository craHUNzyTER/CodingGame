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

            DefaultMove();
        }

        private static void DeliverOre()
        {
            foreach (var robot in Helpers.GetRobotsWithOre())
            {
                GameData.OutputCommands[robot.Id] = Output.MoveToHeadquarters(robot);
            }
        }

        private static void RequestRadar()
        {
            if (GameData.VeinCells.Sum(c => c.CurrentOreCount) > 10)
                return;

            var robotInHq = Helpers.GetRobotInHeadquarters();
            if (GameData.RadarCooldown == 0 && robotInHq != null)
            {
                GameData.OutputCommands[robotInHq.Id] = Output.RequestRadar();
                GameData.IsRadarRequested = true;
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
                var coordinateToBury = Helpers.GetClosestCoordinateToBuryRadar(robot);

                if (coordinateToBury != null)
                {
                    GameData.OutputCommands[robot.Id] = Output.Dig(coordinateToBury.X, coordinateToBury.Y);

                    GameData.ExistingRadars.Add(new Radar(default, coordinateToBury));
                }
            }
        }

        private static void DigVeinCells()
        {
            foreach (var robot in Helpers.GetNotAssignedRobots())
            {
                if (GameData.VeinCells.Count == 0)
                    return;

                var closestVeinCell = Helpers.GetClosestVeinCellsToRobot(robot).First();
                closestVeinCell.DigOre();

                var coordinate = closestVeinCell.Coordinate;
                GameData.OutputCommands[robot.Id] = Output.Dig(coordinate.X, coordinate.Y);
            }
        }

        private static void DefaultMove()
        {
            var closestRobotToHq = Helpers.GetClosestAndNotAssignedRobotToHeadquarters();
            if (!GameData.IsRadarRequested && GameData.RadarCooldown <= 1 && closestRobotToHq != null)
            {
                GameData.OutputCommands[closestRobotToHq.Id] = Output.MoveToHeadquarters(closestRobotToHq);
            }

            foreach (var robot in Helpers.GetNotAssignedRobots())
            {
                if (robot.InHeadquarters)
                {
                    GameData.OutputCommands[robot.Id] = Output.Move(robot.Coordinate.X + 2, robot.Coordinate.Y);
                    continue;
                }

                var unknownCellToDig = Helpers.FindNotDiggedAdjacentCell(robot);
                if (unknownCellToDig != null)
                {
                    GameData.OutputCommands[robot.Id] = Output.Dig(unknownCellToDig.Coordinate.X, unknownCellToDig.Coordinate.Y);
                    continue;
                }

                GameData.OutputCommands[robot.Id] = Output.Move(robot.Coordinate.X + 3, robot.Coordinate.Y);
            }
        }
    }

    public static class Helpers
    {
        public static List<Robot> GetNotAssignedRobots()
        {
            var notAssignedRobots = GameData.MyRobots
                .Where(r => !Output.HasAssignedCommand(r.Id))
                .ToList();

            //Console.Error.WriteLine($"Not assigned robots: {notAssignedRobots.Count}.");

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
                .FirstOrDefault(r => r.InHeadquarters);
        }

        public static List<Cell> GetClosestVeinCellsToRobot(Robot robot)
        {
            return GameData.VeinCells
                .GroupBy(c => CalculateDistance(robot.Coordinate, c.Coordinate))
                .OrderBy(gr => gr.Key)
                .First()
                .Select(gr => gr)
                .ToList();
        }

        public static List<Cell> GetVeinCellsInRadarRange(Radar radar)
        {
            return GameData.VeinCells
                .Where(c => CalculateDistance(radar.Coordinate, c.Coordinate) <= Constants.RadarRange)
                .ToList();
        }

        public static Robot GetClosestAndNotAssignedRobotToHeadquarters()
        {
            var notAssignedRobots = GetNotAssignedRobots();

            if (notAssignedRobots.Count == 0)
                return null;

            return notAssignedRobots
                .GroupBy(r => CalculateDistance(r.Coordinate, new Coordinate(Constants.HeadquartersX, r.Coordinate.Y)))
                .OrderBy(gr => gr.Key)
                .First()
                .Select(gr => gr)
                .First();
        }

        public static Coordinate GetClosestCoordinateToBuryRadar(Robot robot)
        {
            var alreadyBuriedCoordinates = GameData.ExistingRadars
                    .Select(r => r.Coordinate)
                    .ToList();

            var coordinatesToBury = Constants.RadarCoordinates
                .Except(alreadyBuriedCoordinates)
                .ToList();

            if (coordinatesToBury.Count == 0)
                return null;

            return coordinatesToBury
                .GroupBy(c => CalculateDistance(robot.Coordinate, c))
                .OrderBy(gr => gr.Key)
                .First()
                .Select(gr => gr)
                .First();
        }

        public static Cell FindNotDiggedAdjacentCell(Robot robot)
        {
            var x = robot.Coordinate.X;
            var y = robot.Coordinate.Y;
            var map = GameData.Map;

            if (x > 1 && x < 29 && map[y, x - 1].HasNotHole)
            {
                return map[y, x - 1];
            }

            if (x > 1 && x < 29 && map[y, x + 1].HasNotHole)
            {
                return map[y, x + 1];
            }

            if (y > 0 && x < 14 && map[y - 1, x].HasNotHole)
            {
                return map[y - 1, x];
            }

            if (y > 0 && x < 14 && map[y + 1, x].HasNotHole)
            {
                return map[y + 1, x];
            }

            return null;
        }

        private static int CalculateDistance(Coordinate first, Coordinate second)
        {
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y);
        }
    }

    static class GameData
    {
        public static void InitializeGame()
        {
            Map = new Cell[Constants.MapHeight, Constants.MapWidth];
            Cells = new List<Cell>(Constants.MapHeight * Constants.MapWidth);

            for (int y = 0; y < Constants.MapHeight; y++)
            {
                for (int x = 0; x < Constants.MapWidth; x++)
                {
                    var cell = new Cell(new Coordinate(x, y), Constants.UnknownOreCount, false);

                    Map[y, x] = cell;
                    Cells.Add(cell);
                }
            }
        }

        public static void InitializeTurn()
        {
            MyRobots = new List<Robot>(Constants.OneTeamRobotsCount);
            OpponentRobots = new List<Robot>(Constants.OneTeamRobotsCount);

            ExistingRadars = new List<Radar>();
            IsRadarRequested = false;

            OutputCommands = new string[Constants.OneTeamRobotsCount];
        }

        public static bool IsRadarRequested { get; set; }
        public static int RadarCooldown { get; set; }
        public static int TrapCooldown { get; set; }

        public static Cell[,] Map { get; set; }
        public static List<Cell> Cells { get; set; }
        public static List<Cell> VeinCells { get; set; }

        public static List<Robot> MyRobots { get; set; }
        public static List<Robot> OpponentRobots { get; set; }

        public static List<Radar> ExistingRadars { get; set; }

        public static string[] OutputCommands { get; set; } // output for every turn
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

    public class Robot
    {
        private readonly EntityType _type;

        public int Id { get; }
        public Coordinate Coordinate { get; }
        public EntityType Item { get; }

        #region Constructors and overriden methods

        public Robot(int id, EntityType type, Coordinate coordinate, EntityType item)
        {
            Id = id < 5 ? id : id - 5;
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

        public bool InHeadquarters => Coordinate.X == Constants.HeadquartersX;
    }

    public class Radar
    {
        public int Id { get; }
        public Coordinate Coordinate { get; }

        #region Constructors and overriden methods

        public Radar(int id, Coordinate coordinate)
        {
            Id = id;
            Coordinate = coordinate;
        }

        public override string ToString()
        {
            return $"Radar. Id={Id}. {Coordinate}";
        }

        public override bool Equals(object obj)
        {
            Radar other = obj as Radar;

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
    }

    public class Cell
    {
        public Coordinate Coordinate { get; }

        public int CurrentOreCount { get; private set; }
        public int InitialOreCount { get; private set; }
        public bool HasHole { get; private set; }

        #region Constructors and overriden methods

        public Cell(Coordinate coordinate, int oreCount, bool hasHole)
        {
            Coordinate = coordinate;
            CurrentOreCount = oreCount;
            InitialOreCount = oreCount;
            HasHole = hasHole;
        }

        public override string ToString()
        {
            return $"{Coordinate}, CurrentOre={CurrentOreCount}, Hole={HasHole}";
        }

        #endregion Constructors and overriden methods

        public bool HasOre => CurrentOreCount > 0;
        public bool HasNotHole => !HasHole;

        public void SetCurrentState(int oreCount, bool hasHole)
        {
            HasHole = hasHole;
            CurrentOreCount = oreCount;

            if (InitialOreCount == Constants.UnknownOreCount)
            {
                InitialOreCount = oreCount;
            }
        }

        public void DigOre()
        {
            CurrentOreCount--;
            if (!HasOre)
            {
                GameData.VeinCells.Remove(this);
            }
        }
    }

    public enum EntityType
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

        public static int UnknownOreCount = -1;
        public static int RadarRange = 4;

        public static List<Coordinate> RadarCoordinates = new List<Coordinate>
        {
            new Coordinate(6, 3),
            new Coordinate(6, 11),

            new Coordinate(11, 7),

            new Coordinate(16, 3),
            new Coordinate(16, 11),

            new Coordinate(21, 7),

            new Coordinate(26, 3),
            new Coordinate(26, 11)
        };

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
                    string ore = _inputs[2 * x]; // amount of ore or "?" if unknown
                    int oreCount = ore == "?" ? Constants.UnknownOreCount : int.Parse(ore);

                    int hole = int.Parse(_inputs[2 * x + 1]); // 1 if cell has a hole
                    bool hasHole = hole == 1;

                    GameData.Map[y, x].SetCurrentState(oreCount, hasHole);
                }
            }

            GameData.VeinCells = GameData.Cells
                .Where(c => c.HasOre)
                .OrderBy(c => c.Coordinate.X)
                .ToList();
            //Console.Error.WriteLine($"Vein cells count: {GameData.VeinCells.Count}");

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
                else if (type == EntityType.Radar)
                {
                    var radar = new Radar(id, coordinate);
                    var detectedVeins = Helpers.GetVeinCellsInRadarRange(radar);
                    if (detectedVeins.Count == 0)
                    {
                        var coordinateToRemove = Constants.RadarCoordinates.FirstOrDefault(c => Equals(c, radar.Coordinate));
                        if (coordinateToRemove != null)
                        {
                            Console.Error.WriteLine($"Radar coordinates count {Constants.RadarCoordinates.Count}.");
                            Constants.RadarCoordinates.Remove(radar.Coordinate);
                            Console.Error.WriteLine($"Radar coordinates count {Constants.RadarCoordinates.Count}.");
                        }
                    }

                    Console.Error.WriteLine($"Radar {radar} has detected {detectedVeins.Count} veins.");
                    //foreach (var detectedVein in detectedVeins)
                    //{
                    //    Console.Error.WriteLine($"Vein {detectedVein}.");
                    //}

                    GameData.ExistingRadars.Add(radar);
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

        public static string MoveToHeadquarters(Robot robot)
        {
            return Move(Constants.HeadquartersX, robot.Coordinate.Y);
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