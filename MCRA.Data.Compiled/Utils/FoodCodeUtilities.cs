using System.Text.RegularExpressions;

namespace MCRA.Data.Compiled.Utils {
    public static partial class FoodCodeUtilities {

        [GeneratedRegex(@"^[A-Za-z0-9\-]{5}(#(F\d{2}.[A-Za-z0-9\-]{5})(\$F\d{2}.[A-Za-z0-9\-]{5})*)?$")]
        private static partial Regex IsFoodEx2Regex();

        [GeneratedRegex(@"^([A-Za-z0-9\-]{5,})#((F\d{2}.[A-Za-z0-9\-]{5})(\$F\d{2}.[A-Za-z0-9\-]{5})*)+$")]
        private static partial Regex IsFoodEx2WithFacetsRegex();

        [GeneratedRegex(@"^((F\d{2}.[A-Za-z0-9\-]{5})(\$F\d{2}.[A-Za-z0-9\-]{5})*)+$")]
        private static partial Regex FullFoodEx2FacetStringRegex();

        [GeneratedRegex(@"F\d{2}.[A-Za-z0-9\-]{5}$")]
        private static partial Regex LastFoodEx2FacetRegex();

        [GeneratedRegex(@"F\d{2}.[A-Za-z0-9\-]{5}")]
        private static partial Regex FoodEx2FacetRegex();

        /// <summary>
        /// Returns whether the food code is a valid FoodEx 2 code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static bool IsFoodEx2Code(string foodCode) {
            return IsFoodEx2Regex().IsMatch(foodCode);
        }

        /// <summary>
        /// Returns whether the food code is a valid FoodEx 2 code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static bool IsCodeWithFoodEx2Facets(string foodCode) {
            return IsFoodEx2WithFacetsRegex().IsMatch(foodCode);
        }

        /// <summary>
        /// Returns whether the code is a valid FoodEx 2 facet string.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsFoodEx2FacetString(string code) {
            return FullFoodEx2FacetStringRegex().IsMatch(code);
        }

        /// <summary>
        /// Returns the food base code of a food code with FoodEx 2 facets (i.e., the
        /// food code without the facet code). If the code does not contain FoodEx 2
        /// facets, then it returns the original code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static string GetFoodEx2BaseCode(string foodCode) {
            if (IsCodeWithFoodEx2Facets(foodCode)) {
                var regex = new Regex(@"^[A-Za-z0-9\-]{5,}", RegexOptions.Compiled);
                var match = regex.Match(foodCode);
                return match.Value;
            } else if (foodCode.Contains('#')) {
                return foodCode[..foodCode.IndexOf("#")];
            } else {
                return foodCode;
            }
        }

        /// <summary>
        /// Gets the facet string of the FoodEx2 food code. Returns null
        /// if the specified string is not a food code with FoodEx2 facets.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static string GetFoodEx2FacetString(string foodCode) {
            if (IsCodeWithFoodEx2Facets(foodCode)) {
                return foodCode[(foodCode.IndexOf("#")+1)..];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns the last facet code of the FoodEx 2 code with facets or an
        /// empty string if the foodcode is not a FoodEx 2 code or does not contain
        /// facets.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static string GetLastFoodEx2FacetCode(string foodCode) {
            if (IsCodeWithFoodEx2Facets(foodCode)) {
                var match = LastFoodEx2FacetRegex().Match(foodCode);
                return match.Value;
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns all facet codes of the FoodEx 2 code with facets or an
        /// empty string if the foodcode is not a FoodEx 2 code or does not contain
        /// facets.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static List<string> GetFoodEx2FacetCodes(string foodCode) {
            if (IsCodeWithFoodEx2Facets(foodCode)) {
                var matches = FoodEx2FacetRegex().Matches(foodCode);
                return matches.Cast<Match>().Select(m => m.Value).ToList();
            } else {
                return [];
            }
        }

        /// <summary>
        /// Returns all facet codes of the FoodEx 2 facet string or an
        /// empty list if the string is not recognized as a facet string.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static List<string> ParseFacetString(string foodCode) {
            if (IsFoodEx2FacetString(foodCode)) {
                var matches = FoodEx2FacetRegex().Matches(foodCode);
                return matches.Cast<Match>().Select(m => m.Value).ToList();
            } else {
                return [];
            }
        }

        /// <summary>
        /// Returns the stripped version of the given FoodEx 2 code in which all occurences
        /// of the provided facet are stripped. The facet code should be defined fully as
        /// "F##.#####". If no occurence is found or the food code is not in FoodEx 2 format
        /// then the original food code is returned.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static string StripFacetFromFoodEx2Code(string foodCode, string facetCode) {
            if (IsCodeWithFoodEx2Facets(foodCode)) {
                var baseCode = GetFoodEx2BaseCode(foodCode);
                var matches = FoodEx2FacetRegex().Matches(foodCode);
                var facetCodes = matches.Cast<Match>().Select(m => m.Value).Where(f => f != facetCode);
                if (facetCodes.Any()) {
                    return $"{baseCode}#{string.Join("$", facetCodes)}";
                } else {
                    return baseCode;
                }
            } else {
                return foodCode;
            }
        }

        /// <summary>
        /// Returns the FoodEx 2 facet code and facet descriptor code from the full facet
        /// code. The full facet code should be a valid FoodEx 2 full facet code. If this
        /// is not the case, then both values are null.
        /// </summary>
        /// <param name="fullFacetCode"></param>
        /// <returns></returns>
        public static (string FacetCode, string DescriptorCode) SplitFoodEx2FoodFacetCode(string fullFacetCode) {
            if (Regex.IsMatch(fullFacetCode, @"F\d{2}.[A-Za-z0-9\-]{5}")) {
                return (fullFacetCode.Split('.').First(), fullFacetCode.Split('.').Last());
            }
            return (null, null);
        }

        /// <summary>
        /// Checks for occurrence of the '-' character specifying a processing type substring
        /// in the food code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static bool IsProcessedFood(string foodCode) {
            if (string.IsNullOrEmpty(foodCode)) {
                return false;
            } else {
                var ix = foodCode.IndexOf('-');
                return ix > 0 && ix < foodCode.Length - 1;
            }
        }

        /// <summary>
        /// Gets the processing codes of the (processed) food code. Returns null if the food
        /// code is not a processed food code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static List<string> GetFoodProcessingParts(string foodCode) {
            if (IsProcessedFood(foodCode)) {
                var parts = foodCode.Split('-');
                if (parts.Any()) {
                    return parts.Skip(1).ToList();
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the food base code of the processed food code. Returns null if the food code
        /// is not a processed food code.
        /// </summary>
        /// <param name="foodCode"></param>
        /// <returns></returns>
        public static string GetProcessedFoodBaseCode(string foodCode) {
            if (IsProcessedFood(foodCode)) {
                var parts = foodCode.Split('-');
                if (parts.Any()) {
                    return parts[0];
                }
            }
            return null;
        }
    }
}
