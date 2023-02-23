using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Population : IStrongEntity {

        public Population() {
        }

        private string _name;

        public string Code { get; set; }

        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Code;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double NominalBodyWeight { get; set; }

        public BodyWeightUnit BodyWeightUnit { 
            get {
                return BodyWeightUnit.kg;
            }
        }

        public Dictionary<string, PopulationIndividualPropertyValue> PopulationIndividualPropertyValues { get; set; }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
