using System.Text.RegularExpressions;
using MCRA.General;
using MCRA.Utils.DateTimes;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Food : StrongEntity {

        public Food() {
            //FoodConsumptionQuantifications key is the Unit
            FoodConsumptionQuantifications = new Dictionary<string, FoodConsumptionQuantification>(StringComparer.OrdinalIgnoreCase);
            FoodOrigins = [];
            FoodUnitWeights = [];
            ProcessingTypes = [];
            Children = [];
            FoodFacets = [];
        }

        public Food(string code) : this() {
            Code = code;
        }

        public string AlternativeName { get; set; }

        public Dictionary<string, FoodConsumptionQuantification> FoodConsumptionQuantifications { get; set; }
        public MarketShare MarketShare { get; set; }

        public Food BaseFood { get; set; }
        public Food Parent { get; set; }
        public ICollection<Food> Children { get; set; }
        public ICollection<FoodFacet> FoodFacets { get; set; }
        public ICollection<ProcessingType> ProcessingTypes { get; set; }

        public FoodProperty Properties { get; set; }
        public ICollection<FoodUnitWeight> FoodUnitWeights { get; set; }
        public FoodUnitWeight DefaultUnitWeightEp { get; set; }
        public FoodUnitWeight DefaultUnitWeightRac { get; set; }
        public ICollection<FoodOrigin> FoodOrigins { get; set; }

        public string ProcessingFacetCode() {
            if (ProcessingTypes?.Count > 0) {
                return string.Join("-", ProcessingTypes.Select(p => p.Code));
            }
            return null;
        }

        /// <summary>
        /// Retrieves the default unit weight of the specified value type (RAC/EP).
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public QualifiedValue GetDefaultUnitWeight(UnitWeightValueType valueType) {
            if (valueType == UnitWeightValueType.UnitWeightEp && DefaultUnitWeightEp != null) {
                return DefaultUnitWeightEp?.QualifiedValue;
            } else if (valueType == UnitWeightValueType.UnitWeightRac && DefaultUnitWeightRac != null) {
                return DefaultUnitWeightRac?.QualifiedValue;
            } else if (Properties != null && Properties.UnitWeight.HasValue && !double.IsNaN(Properties.UnitWeight.Value)) {
                return new QualifiedValue(Properties.UnitWeight.Value);
            }
            return null;
        }

        /// <summary>
        /// Retrieves the unit weight of the specified type (RAC/EP) for the specified location.
        /// If a location specific unit weight cannot be found, then fallback to average of other
        /// locations or else the default unit weight.
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public QualifiedValue GetUnitWeight(UnitWeightValueType valueType, string location) {
            var unitWeights = FoodUnitWeights?.Where(r => r.ValueType == valueType);
            var unitWeight = unitWeights?.FirstOrDefault(r => location != null && r.Location.Equals(location, StringComparison.OrdinalIgnoreCase))?.QualifiedValue;
            if ((unitWeight?.IsNan() ?? true) && (unitWeights?.Any() ?? false)) {
                unitWeight = unitWeights.Select(r => r.QualifiedValue).Average();
            }
            if (unitWeight?.IsNan() ?? true) {
                unitWeight = GetDefaultUnitWeight(valueType);
            }
            return unitWeight;
        }

        /// <summary>
        /// The unit in which the food's unit weight is specified.
        /// </summary>
        public ConsumptionUnit UnitWeightUnit => ConsumptionUnit.g;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketLocation"></param>
        /// <param name="timeRange"></param>
        /// <returns></returns>
        public List<FoodOrigin> GetMostSpecificFoodOrigins(string marketLocation, TimeRange timeRange) {
            IEnumerable<FoodOrigin> foodOrigins = [];
            var mostSignificatantRegion = new Regex(@"\$[^\$]+$");

            if (marketLocation != null && timeRange != null) { //find matches inside marketLocation withing timeRange
                foodOrigins = FoodOrigins
                    .Where(fo => fo.MarketLocation == marketLocation && fo.Period.OverlapsWith(timeRange));

            }
            if (marketLocation != null && !foodOrigins.Any()) { //find matches for superregions of marketLocation within timeRange
                var seachStr = (string)marketLocation.Clone();
                while (!foodOrigins.Any() && seachStr.Contains('$')) {
                    foodOrigins = FoodOrigins
                        .Where(fo => fo.MarketLocation == seachStr && fo.Period.OverlapsWith(timeRange));
                }
            }

            if (marketLocation != null && !foodOrigins.Any()) { //find matches inside and for all subregions of marketLocation
                foodOrigins = FoodOrigins
                    .Where(fo => fo.MarketLocation == marketLocation);
            }

            if (marketLocation != null && !foodOrigins.Any()) { //find matches for superregions of marketLocation
                var seachStr = (string)marketLocation.Clone();
                while (!foodOrigins.Any() && seachStr.Contains('$')) {
                    foodOrigins = FoodOrigins
                        .Where(fo => fo.MarketLocation == seachStr);
                }
            }

            if (marketLocation == null && timeRange == null) {
                foodOrigins = FoodOrigins
                    .Where(fo => fo.MarketLocation == null && fo.Period == null);
            }

            return foodOrigins.ToList();
        }
    }
}
