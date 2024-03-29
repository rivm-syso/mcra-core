using MCRA.Utils.DateTimes;
using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodSurvey:IStrongEntity {

        private string _name;

        public FoodSurvey() {
            Individuals = new HashSet<Individual>();
        }

        public string Code { get; set; }
        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Code;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }
        public string Location { get; set; }
        public string BodyWeightUnitString { get; set; }
        public string AgeUnitString { get; set; }
        public string ConsumptionUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? NumberOfSurveyDays { get; set; }
        public string IdPopulation { get; set; }

        public ICollection<Individual> Individuals { get; set; }

        public TimeRange Period {
            get {
                if (StartDate.HasValue && EndDate.HasValue) {
                    return new TimeRange(StartDate.Value, EndDate.Value);
                } else {
                    return null;
                }
            }
        }

        public BodyWeightUnit BodyWeightUnit {
            get {
                return BodyWeightUnitConverter.FromString(BodyWeightUnitString, BodyWeightUnit.kg);
            }
        }

        public ConsumptionUnit ConsumptionUnit {
            get {
                string[] kilogramAliases = new string[] { "kg", "kilogram", "kilogr" };
                string[] gramAliases = new string[] { "g", "gram", "gr" };
                if (ConsumptionUnitString != null) {
                    var unit = ConsumptionUnitString.Trim().Replace(" ", string.Empty).ToLower();
                    if (kilogramAliases.Contains(unit)) {
                        return ConsumptionUnit.kg;
                    } else if (gramAliases.Contains(unit)) {
                        return ConsumptionUnit.g;
                    }
                }
                return ConsumptionUnit.g;
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
