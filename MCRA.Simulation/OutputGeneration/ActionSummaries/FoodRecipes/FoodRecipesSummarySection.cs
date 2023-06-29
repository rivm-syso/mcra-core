using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodRecipesSummarySection : SummarySection {

        public List<FoodRecipesSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<FoodTranslation> recipes, ICollection<Food> allFoods, ICollection<ProcessingType> processingTypes) {
            var recipesLookup = recipes.ToLookup(c => c.FoodFrom.Code);
            var foodsLookup = allFoods.ToDictionary(c => c.Code);
            var processingTypesLookup = processingTypes.ToDictionary(r => r.Code);

            var allToFoods = recipes.Select(r => r.FoodTo).ToHashSet();
            var allFromFoods = recipes.Select(r => r.FoodFrom).ToHashSet();
            var endProducts = recipes.Select(r => r.FoodTo).Where(r => !allFromFoods.Contains(r)).ToHashSet();
            var startProducts = recipes.Select(r => r.FoodFrom).Where(r => !allToFoods.Contains(r)).ToHashSet();

            Records = startProducts
                .SelectMany(food => {
                    var foodTrace = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var recursiveObjects = fillRecursive(food.Code, recipesLookup, foodsLookup, processingTypesLookup, foodTrace);
                    var conversions = recursiveObjects.SelectMany(r => readRecursive(r)).ToList();
                    return conversions;
                })
                .OrderBy(c => c.AsEatenRecipeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.AsEatenRecipeCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.ConvertedRecipeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.ConvertedRecipeCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<RecursiveRecipe> fillRecursive(string foodFromCode, ILookup<string, FoodTranslation> recipesLookup, Dictionary<string, Food> foodsLookup, IDictionary<string, ProcessingType> processingTypesLookup, HashSet<string> foodTrace) {
            var result = new List<RecursiveRecipe>();
            foodsLookup.TryGetValue(foodFromCode, out var foodFrom);
            var foodFromName = getFoodName(foodFromCode, foodsLookup, processingTypesLookup);
            if (recipesLookup.Contains(foodFromCode)) {
                // Recipe found
                foreach (var item in recipesLookup[foodFromCode]) {
                    var foodToCode = item.FoodTo.Code;
                    var foodToName = getFoodName(foodToCode, foodsLookup, processingTypesLookup);
                    var record = new RecursiveRecipe {
                        FoodFromCode = foodFromCode,
                        FoodFromName = foodFromName,
                        FoodToCode = foodToCode,
                        FoodToName = foodToName,
                        Proportion = item.Proportion
                    };
                    var localTrace = foodTrace.Union(new List<string>() { foodFromCode }).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (!localTrace.Contains(foodToCode)) {
                        record.Children = fillRecursive(foodToCode, recipesLookup, foodsLookup, processingTypesLookup, localTrace);
                    } else {
                        record.Circular = true;
                    }
                    result.Add(record);
                }
            } else if (foodFromCode.Contains('-')) {
                // Processing character found; try default processing
                var ix = foodFromCode.LastIndexOf('-');
                var foodToCode = foodFromCode.Substring(0, ix);
                var foodToName = getFoodName(foodToCode, foodsLookup, processingTypesLookup);
                var record = new RecursiveRecipe {
                    FoodFromCode = foodFromCode,
                    FoodFromName = foodFromName,
                    FoodToCode = foodToCode,
                    FoodToName = foodToName,
                    Proportion = 1D
                };
                var localTrace = foodTrace.Union(new List<string>() { foodFromCode }).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (!localTrace.Contains(foodToCode)) {
                    record.Children = fillRecursive(foodToCode, recipesLookup, foodsLookup, processingTypesLookup, localTrace);
                } else {
                    record.Circular = true;
                }
                result.Add(record);
            }
            return result;
        }

        /// <summary>
        /// Read the food as eaten and last food of the recipe
        /// </summary>
        /// <param name="recursiveRecipe"></param>
        /// <param name="trace"></param>
        /// <param name="proportion"></param>
        /// <param name="traces"></param>
        private static List<FoodRecipesSummaryRecord> readRecursive(RecursiveRecipe recursiveRecipe) {
            var result = new List<FoodRecipesSummaryRecord>();
            if (recursiveRecipe.Children?.Any() ?? false) {
                foreach (var child in recursiveRecipe.Children) {
                    var flatChildren = readRecursive(child);
                    result.AddRange(flatChildren);
                }
                foreach (var item in result) {
                    var intermediateCodes = new List<string>() { recursiveRecipe.FoodFromCode };
                    intermediateCodes.AddRange(item.IntermediateCodes);
                    item.AsEatenRecipeName = recursiveRecipe.FoodFromName;
                    item.AsEatenRecipeCode = recursiveRecipe.FoodFromCode;
                    item.IntermediateCodes = intermediateCodes;
                    item.Proportion = item.Proportion * recursiveRecipe.Proportion;
                }
            } else {
                var record = new FoodRecipesSummaryRecord() {
                    AsEatenRecipeName = recursiveRecipe.FoodFromName,
                    AsEatenRecipeCode = recursiveRecipe.FoodFromCode,
                    ConvertedRecipeName = recursiveRecipe.FoodToName,
                    ConvertedRecipeCode = recursiveRecipe.FoodToCode,
                    Proportion = recursiveRecipe.Proportion,
                    IntermediateCodes = new List<string>() { recursiveRecipe.FoodFromCode },
                };
                result.Add(record);
            }
            return result;
        }

        private static string getFoodName(string code, IDictionary<string, Food> foodsLookup, IDictionary<string, ProcessingType> processingTypesLookup) {
            foodsLookup.TryGetValue(code, out var food);
            if (!string.IsNullOrEmpty(food?.Name)) {
                return food.Name;
            } else {
                var split = code.Split('-');
                if (split.Length > 1 && foodsLookup.TryGetValue(split[0], out food) && !string.IsNullOrEmpty(food.Name)) {
                    var processingTypes = split.Skip(1).Select(r => processingTypesLookup.TryGetValue(r, out var pf) ? pf.Name : $"processed #{r}");
                    var name = $"{food.Name} ({string.Join(", ", processingTypes)})";
                    return name;
                }
            }
            return code;
        }

        private static string removeProcessingPart(string text) {
            return new string(text.TakeWhile(c => !c.Equals('-')).ToArray());
        }

        private class RecursiveRecipe {
            public string FoodToCode { get; set; }
            public string FoodFromCode { get; set; }
            public string FoodToName { get; set; }
            public string FoodFromName { get; set; }
            public double Proportion { get; set; }
            public List<RecursiveRecipe> Children { get; set; }
            public bool Circular { get; set; }
        }
    }
}