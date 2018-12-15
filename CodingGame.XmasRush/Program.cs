using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Help the Christmas elves fetch presents in a magical labyrinth!
 **/

// Write an action using Console.WriteLine()
// To debug: Console.Error.WriteLine("Debug messages...");

namespace CodingGame.XmasRush
{
    class Program
    {
        static void Main(string[] args)
        {
            GameData.Initialize();

            // game loop
            while (true)
            {
                Input.Parse();

                GameData.CurrentRoundInfo = new RoundInfo(GameData.TurnType, GameData.TurnNumber);

                Game.Play();

                GameData.NextRound();
            }
        }
    }

    static class Game
    {
        public static void Play()
        {
            if (GameData.TurnType.IsPushTurn())
            {
                (int id, Direction direction) = CalculatePush();

                Output.Push(id, direction);
            }
            else
            {
                (bool isMoving, IReadOnlyList<Direction> moveDirections) = CalculateMove();

                if (isMoving && moveDirections.Any())
                {
                    Output.Move(moveDirections);
                }
                else
                {
                    Output.Pass();
                }
            }
        }

        #region Push turn

        static (int id, Direction direction) CalculatePush()
        {
            List<Item> items = GameData.MyQuests.Select(x => x.Item).ToList();
            Coordinate myPlayerCoordinate = GameData.MyPlayer.Position.Coordinate;
            bool isQuestItemInMyHand = items.Any(x => x.IsInMyHand());

            if (!myPlayerCoordinate.IsOnBorder() && !isQuestItemInMyHand)
            {
                return GetPushIdAndDirectionToBorder(myPlayerCoordinate);
            }

            if (isQuestItemInMyHand)
            {
                Item itemInMyHand = items.First(x => x.IsInMyHand());

                if (myPlayerCoordinate.IsOnBorder())
                {
                    return CalculatePushOfMyItemFromMyHandToMyPlayer(myPlayerCoordinate);
                }
            }

            if (items.Any(x => x.IsOnBoard()))
            {
                Item itemToPush = GetItemForPushToBorder(items);
                GameData.CurrentRoundInfo.SelectedItemForPush = itemToPush;

                return GetPushIdAndDirectionToBorder(itemToPush.Coordinate);
            }

            return (3, Direction.RIGHT);
        }

        static (int id, Direction direction) CalculatePushOfMyItemFromMyHandToMyPlayer(Coordinate myPlayerCoordinate)
        {
            if (myPlayerCoordinate.IsVerticalBorder())
            {
                if (myPlayerCoordinate.IsLeftBorder())
                {
                    return (myPlayerCoordinate.Y, Direction.LEFT);
                }

                return (myPlayerCoordinate.Y, Direction.RIGHT);
            }

            if (myPlayerCoordinate.IsTopBorder())
            {
                return (myPlayerCoordinate.X, Direction.UP);
            }

            return (myPlayerCoordinate.X, Direction.DOWN);
        }

        static (int id, int shortestDistance, Direction direction) CalculatePushOfCoordinateToBorders(Coordinate itemCoordinate)
        {
            int shortestDistance = 3;
            int id = 3;
            Direction direction = Direction.RIGHT;

            int newDistance = Constants.HighestBorderCoordinate - itemCoordinate.X;
            if (newDistance < shortestDistance)
            {
                id = itemCoordinate.Y;
                shortestDistance = newDistance;
                direction = Direction.RIGHT;
            }

            newDistance = itemCoordinate.X - Constants.LowestBorderCoordinate;
            if (newDistance < shortestDistance)
            {
                id = itemCoordinate.Y;
                shortestDistance = newDistance;
                direction = Direction.LEFT;
            }

            newDistance = Constants.HighestBorderCoordinate - itemCoordinate.Y;
            if (newDistance < shortestDistance)
            {
                id = itemCoordinate.X;
                shortestDistance = newDistance;
                direction = Direction.DOWN;
            }

            newDistance = itemCoordinate.Y - Constants.LowestBorderCoordinate;
            if (newDistance < shortestDistance)
            {
                id = itemCoordinate.X;
                shortestDistance = newDistance;
                direction = Direction.UP;
            }

            return (id, shortestDistance, direction);
        }

        static (int id, Direction direction) GetPushIdAndDirectionToBorder(Coordinate itemCoordinate)
        {
            (int id, int shortestDistance, Direction direction) = CalculatePushOfCoordinateToBorders(itemCoordinate);

            return (id, direction);
        }

        static int GetShortestDistanceToBorder(Coordinate itemCoordinate)
        {
            (int id, int shortestDistance, Direction direction) = CalculatePushOfCoordinateToBorders(itemCoordinate);

            return shortestDistance;
        }

        static Item GetItemClosestToBorder(IEnumerable<Item> items)
        {
            Item item = items.First();
            int shortestDistance = GetShortestDistanceToBorder(item.Coordinate);

            foreach (Item i in items.Skip(1))
            {
                int newDistance = GetShortestDistanceToBorder(i.Coordinate);

                if (newDistance < shortestDistance)
                {
                    item = i;
                }
            }

            return item;
        }

        static Item GetItemForPushToBorder(IEnumerable<Item> items)
        {
            IEnumerable<Item> itemsOnBoard = items.Where(x => x.IsOnBoard());
            Item itemToPush = GetItemClosestToBorder(itemsOnBoard);

            if (GameData.TurnNumber > 1)
            {
                Item previousPushItem = GameData.PreviousRoundInfos.Single(x => x.TurnNumber == GameData.TurnNumber - 2).SelectedItemForPush;

                if (previousPushItem == null)
                {
                    Console.Error.WriteLine($"Item in info wasn't found.");
                }
                else
                {
                    if (itemToPush.Name == previousPushItem.Name && itemToPush.Coordinate.X == previousPushItem.Coordinate.X && itemToPush.Coordinate.Y == previousPushItem.Coordinate.Y)
                    {
                        Console.Error.WriteLine($"Item '{previousPushItem.Name}' wasn't moved.");

                        if (itemsOnBoard.Count() > 1)
                        {
                            itemToPush = GetItemClosestToBorder(itemsOnBoard.Where(x => x.Name != previousPushItem.Name));
                            Console.Error.WriteLine($"Take next one {itemToPush.Name}.");
                        }
                    }
                }
            }

            return itemToPush;
        }

        #endregion Push turn

        #region Move turn

        static (bool isMoving, IReadOnlyList<Direction> moveDirections) CalculateMove()
        {
            Tile startTile = GameData.MyPlayer.Position;

            if (startTile.Siblings.Any())
            {
                var directions = BuildPath(Array.Empty<Direction>(), startTile);

                return (true, directions);
            }

            return (false, Array.Empty<Direction>());
        }

        static IReadOnlyList<Direction> BuildPath(IReadOnlyList<Direction> previousPath, Tile currentTile)
        {
            return Array.Empty<Direction>();
        }

        #endregion Move turn
    }

    static class GameData
    {
        public static void Initialize()
        {
            TurnNumber = 1;
            PreviousRoundInfos = new List<RoundInfo>();
            Map = new Tile[7, 7];
        }

        public static void NextRound()
        {
            TurnNumber++;
            RoundInfo previousRoundInfo = CurrentRoundInfo.Copy();
            PreviousRoundInfos.Add(previousRoundInfo);
        }

        public static int TurnNumber { get; private set; }
        public static TurnType TurnType { get; set; }

        public static Tile[,] Map { get; set; }

        public static Player MyPlayer { get; set; }
        public static Player OpponentPlayer { get; set; }

        public static List<Item> Items { get; set; }
        public static List<Item> MyItems { get; set; }
        public static List<Item> OpponentItems { get; set; }

        public static RoundInfo CurrentRoundInfo { get; set; }
        public static List<RoundInfo> PreviousRoundInfos { get;  private set; }

        public static List<Quest> Quests { get; set; }
        public static List<Quest> MyQuests { get; set; }
        public static List<Quest> OpponentQuests { get; set; }
    }

    class RoundInfo
    {
        public TurnType TurnType { get; private set; }
        public int TurnNumber { get; private set; }
        public Item SelectedItemForPush { get; set; }

        public RoundInfo(TurnType turnType, int turnNumber)
        {
            TurnType = turnType;
            TurnNumber = turnNumber;
        }

        public RoundInfo Copy()
        {
            return new RoundInfo(TurnType, TurnNumber)
            {
                SelectedItemForPush = SelectedItemForPush
            };
        }
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

    class Tile
    {
        public Coordinate Coordinate { get; private set; }
        public bool IsVisited { get; private set; }
        public bool HasUpPath { get; private set; }
        public bool HasRightPath { get; private set; }
        public bool HasDownPath { get; private set; }
        public bool HasLeftPath { get; private set; }
        public Item Item { get; private set; }
        public bool HasItem => Item != null;
        public bool HasMyItem => HasItem && Item.IsMyItem();

        public List<Tile> Siblings { get; private set; }

        public void SetItem(Item item)
        {
            Item = item;
        }

        public void AddSibling(Tile sibling)
        {
            Siblings.Add(sibling);
        }

        public void MarkAsVisited()
        {
            IsVisited = true;
        }

        #region Constructors and overriden methods

        public Tile(Coordinate coordinate, string tilePaths)
        {
            Coordinate = coordinate;
            Item = null;
            Siblings = new List<Tile>();
            IsVisited = false;

            HasUpPath = tilePaths[0] == '1' ? true : false;
            HasRightPath = tilePaths[1] == '1' ? true : false;
            HasDownPath = tilePaths[2] == '1' ? true : false;
            HasLeftPath = tilePaths[3] == '1' ? true : false;
        }

        public override string ToString()
        {
            return $"Coordinate: {Coordinate}, Up = {HasUpPath}, Right = {HasRightPath}, Down = {HasDownPath}, Left = {HasLeftPath}, Item: {Item?.Name}";
        }

        public override bool Equals(object obj)
        {
            Tile other = obj as Tile;

            if (ReferenceEquals(null, obj))
                return false;

            return Coordinate == other.Coordinate;
        }

        public override int GetHashCode()
        {
            return 7333 ^ Coordinate.GetHashCode();
        }

        #endregion Constructors and overriden methods
    }

    class Player
    {
        public int TotalQuestsCount { get; private set; }
        public Tile Position { get; set; }
        public Tile TileInHand { get; set; }

        public Player(int totalQuestsCount, Tile position, Tile tileInHand)
        {
            TotalQuestsCount = totalQuestsCount;
            Position = position;
            TileInHand = tileInHand;
        }
    }

    class Item
    {
        public string Name { get; private set; }
        public Coordinate Coordinate => Tile?.Coordinate;
        public Tile Tile { get; private set; }
        public int PlayerId { get; private set; }

        public void SetTile(Tile itemTile)
        {
            Tile = itemTile;
        }

        public Item(string name, int playerId)
        {
            Name = name;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return $"{Name}, {Coordinate?.ToString()}";
        }
    }

    class Quest
    {
        public Item Item { get; private set; }
        public int PlayerId { get; private set; }

        public Quest(Item item, int playerId)
        {
            Item = item;
            PlayerId = playerId;
        }
    }

    static class Constants
    {
        public const int MaxStepsCount = 20;

        public const int MyPlayerId = 0;
        public const int OpponentId = 1;

        public const int LowestBorderCoordinate = 0;
        public const int CentralCoordinate = 3;
        public const int HighestBorderCoordinate = 6;
        public const int MyHandCoordinate = -1;
        public const int OpponentHandCoordinate = -2;
    }

    enum TurnType
    {
        PUSH,
        MOVE
    }

    enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    #region Read input data
    static class Input
    {
        private static string[] _inputs;

        public static void Parse()
        {
            // Read turn type
            ParseTurnType();

            // Read map
            ParseMap();

            // Read players' info
            ParsePlayersInfo();
            //Console.Error.WriteLine($"My coordinate: {GameData.MyPlayer.Tile.Coordinate}");

            // Read items
            ParseItems();
            //Console.Error.WriteLine($"Items count: {GameData.Items.Count}");

            // Read quests
            ParseQuests();
            //Console.Error.WriteLine($"Quests count: {GameData.Quests.Count}");
        }

        static void ParseTurnType()
        {
            int turnType = int.Parse(Console.ReadLine());

            switch (turnType)
            {
                case 0:
                    GameData.TurnType = TurnType.PUSH;
                    break;
                case 1:
                    GameData.TurnType = TurnType.MOVE;
                    break;
                default:
                    throw new ArgumentException(nameof(turnType));
            }
        }

        static void ParseMap()
        {
            for (int i = 0; i < 7; i++)
            {
                _inputs = Console.ReadLine().Split(' ');
                for (int j = 0; j < 7; j++)
                {
                    string tilePaths = _inputs[j];

                    Tile tile = new Tile(new Coordinate(j, i), tilePaths);
                    GameData.Map[tile.Coordinate.X, tile.Coordinate.Y] = tile;
                }
            }

            Tile topSibling;
            Tile rightSibling;
            Tile bottomSibling;
            Tile leftSibling;

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Tile tile = GameData.Map[i, j];

                    if (tile.HasUpPath && tile.IsTopBorder() == false)
                    {
                        topSibling = GameData.Map[i, j - 1];
                        if (topSibling.HasDownPath)
                            tile.AddSibling(topSibling);
                    }

                    if (tile.HasRightPath && tile.IsRightBorder() == false)
                    {
                        rightSibling = GameData.Map[i + 1, j];
                        if (rightSibling.HasLeftPath)
                            tile.AddSibling(rightSibling);
                    }

                    if (tile.HasDownPath && tile.IsBottomBorder() == false)
                    {
                        bottomSibling = GameData.Map[i, j + 1];
                        if (bottomSibling.HasUpPath)
                            tile.AddSibling(bottomSibling);
                    }

                    if (tile.HasLeftPath && tile.IsLeftBorder() == false)
                    {
                        leftSibling = GameData.Map[i - 1, j];
                        if (leftSibling.HasRightPath)
                            tile.AddSibling(leftSibling);
                    }
                }

                //Tile testTile = GameData.Map[i, 2];
                //Console.Error.WriteLine($"Siblings for tile: {testTile}");
                //foreach(Tile sibling in testTile.Siblings)
                //{
                //    Console.Error.WriteLine($"Sibling: {sibling.Coordinate}");
                //}
            }
        }

        static void ParsePlayersInfo()
        {
            _inputs = Console.ReadLine().Split(' ');
            int myQuestsCount = int.Parse(_inputs[0]); // the total number of quests for a player (hidden and revealed)
            int myX = int.Parse(_inputs[1]);
            int myY = int.Parse(_inputs[2]);
            string myTilePaths = _inputs[3];

            Tile myPosition = GameData.Map[myX, myY];
            Tile myTileInHand = new Tile(new Coordinate(-1, -1), myTilePaths);

            GameData.MyPlayer = new Player(myQuestsCount, myPosition, myTileInHand);
            //Console.Error.WriteLine($"My position: {GameData.MyPlayer.Position}");

            _inputs = Console.ReadLine().Split(' ');
            int opponentQuestsCount = int.Parse(_inputs[0]); // the total number of quests for a player (hidden and revealed)
            int opponentX = int.Parse(_inputs[1]);
            int opponentY = int.Parse(_inputs[2]);
            string opponentTilePaths = _inputs[3];

            Tile opponentPosition = GameData.Map[opponentX, opponentY];
            Tile opponentTileInHand = new Tile(new Coordinate(-2, -2), opponentTilePaths);

            GameData.OpponentPlayer = new Player(opponentQuestsCount, opponentPosition, opponentTileInHand);
            //Console.Error.WriteLine($"Opponent position: {GameData.OpponentPlayer.Position}");
        }

        static void ParseItems()
        {
            int itemsCount = int.Parse(Console.ReadLine()); // the total number of items available on board and on player tiles
            GameData.Items = new List<Item>(itemsCount);

            for (int i = 0; i < itemsCount; i++)
            {
                _inputs = Console.ReadLine().Split(' ');
                string name = _inputs[0];
                int x = int.Parse(_inputs[1]);
                int y = int.Parse(_inputs[2]);
                int playerId = int.Parse(_inputs[3]);

                Tile itemTile;

                Coordinate itemCoordinate = new Coordinate(x, y);
                Item item = new Item(name, playerId);

                if (itemCoordinate.IsOnBoard())
                {
                    itemTile = GameData.Map[x, y];
                }
                else
                {
                    if (itemCoordinate.IsInMyHand())
                    {
                        itemTile = GameData.MyPlayer.TileInHand;
                        Console.Error.WriteLine($"I carry in hands: {item}");
                    }
                    else
                    {
                        itemTile = GameData.OpponentPlayer.TileInHand;
                        Console.Error.WriteLine($"Opponent carries in hands: {item}");
                    }
                }

                itemTile.SetItem(item);
                item.SetTile(itemTile);

                GameData.Items.Add(item);
            }

            GameData.MyItems = GameData.Items.Where(i => i.IsMyItem()).ToList();
            GameData.OpponentItems = GameData.Items.Where(i => i.IsOpponentItem()).ToList();
        }

        static void ParseQuests()
        {
            int questsCount = int.Parse(Console.ReadLine()); // the total number of revealed quests for both players
            GameData.Quests = new List<Quest>(questsCount);

            for (int i = 0; i < questsCount; i++)
            {
                _inputs = Console.ReadLine().Split(' ');
                string itemName = _inputs[0];
                int playerId = int.Parse(_inputs[1]);

                Item item = GameData.Items.Single(x => x.Name == itemName && x.PlayerId == playerId);

                Quest quest = new Quest(item, playerId);
                GameData.Quests.Add(quest);
            }

            GameData.MyQuests = GameData.Quests.Where(i => i.PlayerId == Constants.MyPlayerId).ToList();
            GameData.OpponentQuests = GameData.Quests.Where(i => i.PlayerId == Constants.OpponentId).ToList();
        }
    }

    #endregion Read input data

    static class Output
    {
        public static void Push(int id, Direction direction)
        {
            Console.WriteLine($"PUSH {id} {direction}"); // PUSH <id> <direction>
        }

        public static void Move(IReadOnlyList<Direction> path)
        {
            Console.WriteLine($"MOVE {string.Join(" ", path.Select(x => x.ToString()))}"); // MOVE <direction>
        }

        public static void Pass()
        {
            Console.WriteLine("PASS"); // PASS
        }
    }

    static class Extensions
    {
        #region Coordinate

        public static bool IsOnBoard(this Coordinate coordinate)
        {
            return !coordinate.IsOutsideBoard();
        }

        public static bool IsOutsideBoard(this Coordinate coordinate)
        {
            return coordinate.X < 0
                || coordinate.Y < 0;
        }

        public static bool IsInMyHand(this Coordinate coordinate)
        {
            return coordinate.X == Constants.MyHandCoordinate
                && coordinate.Y == Constants.MyHandCoordinate;
        }

        public static bool IsOnBorder(this Coordinate coordinate)
        {
            return coordinate.IsHorizontalBorder()
                || coordinate.IsVerticalBorder();
        }

        public static bool IsHorizontalBorder(this Coordinate coordinate)
        {
            return coordinate.IsTopBorder()
                || coordinate.IsBottomBorder();
        }

        public static bool IsVerticalBorder(this Coordinate coordinate)
        {
            return coordinate.IsLeftBorder()
                || coordinate.IsRightBorder();
        }

        public static bool IsTopBorder(this Coordinate coordinate)
        {
            return coordinate.Y == Constants.LowestBorderCoordinate;
        }

        public static bool IsBottomBorder(this Coordinate coordinate)
        {
            return coordinate.Y == Constants.HighestBorderCoordinate;
        }

        public static bool IsLeftBorder(this Coordinate coordinate)
        {
            return coordinate.X == Constants.LowestBorderCoordinate;
        }

        public static bool IsRightBorder(this Coordinate coordinate)
        {
            return coordinate.X == Constants.HighestBorderCoordinate;
        }

        #endregion Coordinate

        #region Tile

        public static bool IsTopBorder(this Tile tile)
        {
            return tile.Coordinate.IsTopBorder();
        }

        public static bool IsBottomBorder(this Tile tile)
        {
            return tile.Coordinate.IsBottomBorder();
        }

        public static bool IsLeftBorder(this Tile tile)
        {
            return tile.Coordinate.IsLeftBorder();
        }

        public static bool IsRightBorder(this Tile tile)
        {
            return tile.Coordinate.IsRightBorder();
        }

        #endregion Tile

        #region Player

        #endregion Player

        #region Item

        public static bool IsOnBoard(this Item item)
        {
            return item.Coordinate.IsOnBoard();
        }

        public static bool IsOutsideBoard(this Item item)
        {
            return item.Coordinate.IsOutsideBoard();
        }

        public static bool IsInMyHand(this Item item)
        {
            return item.Coordinate.IsInMyHand();
        }

        public static bool IsMyItem(this Item item)
        {
            return item.PlayerId == Constants.MyPlayerId;
        }

        public static bool IsOpponentItem(this Item item)
        {
            return item.PlayerId == Constants.OpponentId;
        }

        #endregion Item

        #region TurnType

        public static bool IsPushTurn(this TurnType turnType)
        {
            return turnType == TurnType.PUSH;
        }

        #endregion TurnType
    }
}