using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FoodRecipes {
    /// <summary>
    /// FoodRecipes calculator
    /// </summary>
    [TestClass]
    public class FoodRecipesTests {
        /// <summary>
        /// Calculate recipees and fill recursively
        /// </summary>
        [TestMethod]
        public void FoodRecipes_Test1() {
            var foodAsEaten = new List<string> { "A", "C", "H", "X" };
            var recipes = new List<Recipe>();
            recipes.Add(new Recipe() { FromFood = "A", ToFood = "unknownB", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "A", ToFood = "C", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "unknownB", ToFood = "D", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "unknownB", ToFood = "E", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "C", ToFood = "F", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "C", ToFood = "G", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "C", ToFood = "H", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "H", ToFood = "I", Proportion = 50 });
            recipes.Add(new Recipe() { FromFood = "X", ToFood = "X", Proportion = 100 });
            var lookUp = recipes.ToLookup(c => c.FromFood);

            var recipeDict = new Dictionary<string, double>();
            foreach (var food in foodAsEaten) {
                var recursiveObjects = FillRecursive(lookUp, food);
                var traces = new Dictionary<string, double>();
                ReadRecursive(recursiveObjects, string.Empty, 1, traces);
                foreach (var trace in traces) {
                    var result = trace.Key.Split(',');
                    recipeDict[result.First() + " to " + result.Last()] = trace.Value;
                }
            }
        }
        /// <summary>
        /// Strips a food code
        /// </summary>
        [TestMethod]
        public void FoodRecipes_Test2() {
            var s = "unknown ToFood $appelq-12";
            var strippedFoodToCode = s.ReplaceCaseInsensitive("unknown tofood", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private static void ReadRecursive(List<RecursiveObject> recursiveObjects, string trace, double proportion, Dictionary<string, double> traces) {
            foreach (var item in recursiveObjects) {
                var newTrace = trace + item.FromFood + ",";
                var newProportion = proportion * item.Proportion / 100;
                if (item.Children.Count > 0) {
                    ReadRecursive(item.Children, newTrace, newProportion, traces);
                } else {
                    traces[newTrace + item.ToFood] = newProportion;
                }
            }
        }

        private static List<RecursiveObject> FillRecursive(ILookup<string, Recipe> recipes, string fromFood) {
            List<RecursiveObject> recursiveObjects = new List<RecursiveObject>();
            foreach (var item in recipes[fromFood]) {
                if (!item.FromFood.Equals(item.ToFood)) {
                    recursiveObjects.Add(new RecursiveObject {
                        ToFood = item.ToFood,
                        FromFood = item.FromFood,
                        Proportion = item.Proportion,
                        Children = FillRecursive(recipes, item.ToFood)
                    });
                } else {
                    recursiveObjects.Add(new RecursiveObject {
                        ToFood = item.ToFood,
                        FromFood = item.FromFood,
                        Proportion = item.Proportion,
                        Children = new List<RecursiveObject>(),
                    });
                }
            }
            return recursiveObjects;
        }


        public class RecursiveObject {
            public string ToFood { get; set; }
            public string FromFood { get; set; }
            public double Proportion { get; set; }
            public List<RecursiveObject> Children { get; set; }

        }

        public class Recipe {
            public string FromFood { get; set; }
            public string ToFood { get; set; }
            public double Proportion { get; set; }
        }
    }
}
