namespace MCRA.Data.Compiled.Objects {
    public sealed class PopulationIndividualPropertyValue : IEquatable<PopulationIndividualPropertyValue> {

        public string Value { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IndividualProperty IndividualProperty { get; set; }

        public bool IsNumeric() {
            return string.IsNullOrEmpty(Value) || (MinValue != null || MaxValue != null);
        }

        public HashSet<string> CategoricalLevels {
            get {
                return !string.IsNullOrEmpty(Value)
                    ? Value.Split(',').Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : [];
            }
            set {
                Value = string.Join(',', value);
            }
        }

        public bool Equals(PopulationIndividualPropertyValue other) {
            throw new NotImplementedException();
        }
    }
}
