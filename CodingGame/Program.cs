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

            Output.Brew(recipeToBrew);
        }
    }

    static class GameData
    {
        public static List<Recipe> Recipes { get; set; }

        public static void InitializeTurn()
        {
            Recipes = new List<Recipe>();
        }
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
                int tier0Cost = int.Parse(inputs[2]); // tier-0 ingredient change
                int tier1Cost = int.Parse(inputs[3]); // tier-1 ingredient change
                int tier2Cost = int.Parse(inputs[4]); // tier-2 ingredient change
                int tier3Cost = int.Parse(inputs[5]); // tier-3 ingredient change
                int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
                int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
                int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
                bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                var recipe = new Recipe(id, tier0Cost, tier1Cost, tier2Cost, tier3Cost, price);
                
                GameData.Recipes.Add(recipe);
            }

            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
                int inv1 = int.Parse(inputs[1]);
                int inv2 = int.Parse(inputs[2]);
                int inv3 = int.Parse(inputs[3]);
                int score = int.Parse(inputs[4]); // amount of rupees
            }
        }
    }

    static class Output
    {
        public static void Brew(Recipe recipe)
        {
            // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
            Console.WriteLine($"BREW {recipe.Id}");
        }
    }
}