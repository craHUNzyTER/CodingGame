using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingGame
{
    class Program
    {
        static void Main()
        {
            // game loop
            while (true)
            {
                GameData.InitializeTurn();
                Input.ParseTurn();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Game.Play();
            }
        }
    }

    public static class Game
    {
        public static void Play()
        {
            var recipeToBrew = GameData.Recipes.OrderByDescending(r => r.Price).First();

            if (Helpers.IsEnoughIngredients(recipeToBrew, GameData.MyPlayer))
            {
                Output.Brew(recipeToBrew);
                return;
            }

            var spellToCast = FindSpellToCast(recipeToBrew);
            if (spellToCast != null)
            {
                Output.Cast(spellToCast);
                return;
            }

            Output.Rest();
        }

        private static Spell FindSpellToCast(Recipe recipe)
        {
            var tier0IsNeeded = false;
            var tier1IsNeeded = false;
            var tier2IsNeeded = false;

            var player = GameData.MyPlayer;

            if (!Helpers.IsEnoughTier3(recipe, player))
            {
                if (GameData.MyTier3ReadySpells.Any())
                    return GameData.MyTier3ReadySpells.First();

                if (GameData.MyTier3Spells.Any(spell => spell.Delta2 < 0))
                    tier2IsNeeded = true;
                if (GameData.MyTier3Spells.Any(spell => spell.Delta1 < 0))
                    tier1IsNeeded = true;
                if (GameData.MyTier3Spells.Any(spell => spell.Delta0 < 0))
                    tier0IsNeeded = true;
            }

            if (tier2IsNeeded || !Helpers.IsEnoughTier2(recipe, player))
            {
                if (GameData.MyTier2ReadySpells.Any())
                    return GameData.MyTier2ReadySpells.First();

                if (GameData.MyTier2Spells.Any(spell => spell.Delta1 < 0))
                    tier1IsNeeded = true;
                if (GameData.MyTier2Spells.Any(spell => spell.Delta0 < 0))
                    tier0IsNeeded = true;
            }

            if (tier1IsNeeded || !Helpers.IsEnoughTier1(recipe, player))
            {
                if (GameData.MyTier1ReadySpells.Any())
                    return GameData.MyTier1ReadySpells.First();

                if (GameData.MyTier1Spells.Any(spell => spell.Delta0 < 0))
                    tier0IsNeeded = true;
            }

            return (tier0IsNeeded || !Helpers.IsEnoughTier0(recipe, player)) && GameData.MyTier0ReadySpells.Any()
                ? GameData.MyTier0ReadySpells.First()
                : null;
        }
    }

    public static class Helpers
    {
        public static bool IsEnoughIngredients(Recipe recipe, Player player)
        {
            return IsEnoughTier0(recipe, player)
                   && IsEnoughTier1(recipe, player)
                   && IsEnoughTier2(recipe, player)
                   && IsEnoughTier3(recipe, player);
        }

        public static bool IsEnoughTier0(Recipe recipe, Player player)
        {
            return recipe.Tier0Cost + player.Inv0 >= 0;
        }

        public static bool IsEnoughTier1(Recipe recipe, Player player)
        {
            return recipe.Tier1Cost + player.Inv1 >= 0;
        }

        public static bool IsEnoughTier2(Recipe recipe, Player player)
        {
            return recipe.Tier2Cost + player.Inv2 >= 0;
        }

        public static bool IsEnoughTier3(Recipe recipe, Player player)
        {
            return recipe.Tier3Cost + player.Inv3 >= 0;
        }

        public static bool IsEnoughIngredients(Spell spell, Player player)
        {
            return IsEnoughTier0(spell, player)
                   && IsEnoughTier1(spell, player)
                   && IsEnoughTier2(spell, player)
                   && IsEnoughTier3(spell, player);
        }

        public static bool IsEnoughTier0(Spell spell, Player player)
        {
            return spell.Delta0 + player.Inv0 >= 0;
        }

        public static bool IsEnoughTier1(Spell spell, Player player)
        {
            return spell.Delta1 + player.Inv1 >= 0;
        }

        public static bool IsEnoughTier2(Spell spell, Player player)
        {
            return spell.Delta2 + player.Inv2 >= 0;
        }

        public static bool IsEnoughTier3(Spell spell, Player player)
        {
            return spell.Delta3 + player.Inv3 >= 0;
        }

        public static bool IsEnoughSpaceInInventory(Spell spell, Player player)
        {
            return spell.TotalDelta + player.TotalInv <= Constants.InventoryCapacity;
        }
    }

    static class GameData
    {
        public static List<Recipe> Recipes { get; set; }
        public static List<Spell> MyAllSpells { get; set; }
        public static List<Spell> MyReadySpells { get; set; }
        public static List<Spell> MyCastableSpells { get; set; }
        public static List<Spell> MyTier0ReadySpells { get; set; }
        public static List<Spell> MyTier1ReadySpells { get; set; }
        public static List<Spell> MyTier2ReadySpells { get; set; }
        public static List<Spell> MyTier3ReadySpells { get; set; }
        public static List<Spell> MyTier0Spells { get; set; }
        public static List<Spell> MyTier1Spells { get; set; }
        public static List<Spell> MyTier2Spells { get; set; }
        public static List<Spell> MyTier3Spells { get; set; }
        public static Player MyPlayer { get; set; }
        public static Player OpponentPlayer { get; set; }

        public static void InitializeTurn()
        {
            Recipes = new List<Recipe>();
            MyAllSpells = new List<Spell>();
        }
    }

    public class Player
    {
        public int Inv0 { get; }
        public int Inv1 { get; }
        public int Inv2 { get; }
        public int Inv3 { get; }
        public int TotalInv { get; }

        #region Constructors and overriden methods

        public Player(int inv0, int inv1, int inv2, int inv3)
        {
            Inv0 = inv0;
            Inv1 = inv1;
            Inv2 = inv2;
            Inv3 = inv3;
            TotalInv = Inv0 + Inv1 + Inv2 + Inv3;
        }

        #endregion Constructors and overriden methods
    }

    public class Recipe
    {
        public int Id { get; }
        public int Tier0Cost { get; }
        public int Tier1Cost { get; }
        public int Tier2Cost { get; }
        public int Tier3Cost { get; }

        public int Price { get; }

        #region Constructors and overriden methods

        public Recipe(int id, int tier0Cost, int tier1Cost, int tier2Cost, int tier3Cost, int price)
        {
            Id = id;
            Tier0Cost = tier0Cost;
            Tier1Cost = tier1Cost;
            Tier2Cost = tier2Cost;
            Tier3Cost = tier3Cost;
            Price = price;
        }

        public override bool Equals(object obj)
        {
            Recipe other = obj as Recipe;

            if (ReferenceEquals(null, obj))
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 7333 ^ Id.GetHashCode();
        }

        #endregion Constructors and overriden methods
    }

    public class Spell
    {
        public int Id { get; }
        public int Delta0 { get; }
        public int Delta1 { get; }
        public int Delta2 { get; }
        public int Delta3 { get; }
        public int TotalDelta { get; }
        public bool Castable { get; }

        #region Constructors and overriden methods

        public Spell(int id, int delta0, int delta1, int delta2, int delta3, bool castable)
        {
            Id = id;
            Delta0 = delta0;
            Delta1 = delta1;
            Delta2 = delta2;
            Delta3 = delta3;
            TotalDelta = Delta0 + Delta1 + Delta2 + Delta3;
            Castable = castable;
        }

        public override bool Equals(object obj)
        {
            Spell other = obj as Spell;

            if (ReferenceEquals(null, obj))
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 7333 ^ Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Spell Id={Id}, Castable={Castable}";
        }

        #endregion Constructors and overriden methods
    }

    public static class Constants
    {
        public const string Cast = "CAST";
        public const string Brew = "BREW";
        public const int InventoryCapacity = 10;
    }

    static class Input
    {
        public static void ParseTurn()
        {
            int actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play

            string[] inputs;
            for (int i = 0; i < actionCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // the unique ID of this spell or recipe
                string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
                int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
                int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
                int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
                int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
                int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
                int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
                int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
                bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                switch (actionType)
                {
                    case Constants.Cast:
                    {
                        var spell = new Spell(id, delta0, delta1, delta2, delta3, castable);
                        GameData.MyAllSpells.Add(spell);
                        continue;
                    }
                    case Constants.Brew:
                    {
                        var recipe = new Recipe(id, delta0, delta1, delta2, delta3, price);
                        GameData.Recipes.Add(recipe);
                        continue;
                    }
                }
            }

            inputs = Console.ReadLine().Split(' ');
            int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
            int inv1 = int.Parse(inputs[1]);
            int inv2 = int.Parse(inputs[2]);
            int inv3 = int.Parse(inputs[3]);
            int score = int.Parse(inputs[4]); // amount of rupees

            GameData.MyPlayer = new Player(inv0, inv1, inv2, inv3);

            inputs = Console.ReadLine().Split(' ');
            inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
            inv1 = int.Parse(inputs[1]);
            inv2 = int.Parse(inputs[2]);
            inv3 = int.Parse(inputs[3]);
            score = int.Parse(inputs[4]); // amount of rupees

            GameData.OpponentPlayer = new Player(inv0, inv1, inv2, inv3);

            GameData.MyCastableSpells = GameData.MyAllSpells.Where(spell => spell.Castable).ToList();
            
            GameData.MyTier0Spells = GameData.MyAllSpells.Where(spell => spell.Delta0 > 0).ToList();
            GameData.MyTier1Spells = GameData.MyAllSpells.Where(spell => spell.Delta1 > 0).ToList();
            GameData.MyTier2Spells = GameData.MyAllSpells.Where(spell => spell.Delta2 > 0).ToList();
            GameData.MyTier3Spells = GameData.MyAllSpells.Where(spell => spell.Delta3 > 0).ToList();

            GameData.MyReadySpells = GameData.MyCastableSpells
                .Where(spell => Helpers.IsEnoughIngredients(spell, GameData.MyPlayer) && Helpers.IsEnoughSpaceInInventory(spell, GameData.MyPlayer))
                .ToList();
            GameData.MyTier0ReadySpells = GameData.MyReadySpells.Where(spell => spell.Delta0 > 0).ToList();
            GameData.MyTier1ReadySpells = GameData.MyReadySpells.Where(spell => spell.Delta1 > 0).ToList();
            GameData.MyTier2ReadySpells = GameData.MyReadySpells.Where(spell => spell.Delta2 > 0).ToList();
            GameData.MyTier3ReadySpells = GameData.MyReadySpells.Where(spell => spell.Delta3 > 0).ToList();
        }
    }

    static class Output
    {
        public static void Brew(Recipe recipe)
        {
            // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
            Console.WriteLine($"BREW {recipe.Id}");
        }

        public static void Cast(Spell spell)
        {
            Console.WriteLine($"CAST {spell.Id}");
        }

        public static void Rest()
        {
            Console.WriteLine("REST");
        }

        public static void Wait()
        {
            Console.WriteLine("WAIT");
        }
    }
}