using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IndividualProperty : StrongEntity {
        public IndividualProperty() {
            CategoricalLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> CategoricalLevels { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public IndividualPropertyType PropertyType { get; set; }

        public PropertyLevelType PropertyLevel { get; set; }

        public bool IsAgeProperty => nameEquals("Age");

        public bool IsSexProperty => nameEquals("Sex", "Gender");

        public bool IsHeightProperty => nameEquals("Height");

        public bool IsBsaProperty => nameEquals("BSA");

        private bool nameEquals(params string[] compareValues) =>
            compareValues.Any(s => string.Equals(Name, s, StringComparison.OrdinalIgnoreCase));

        public bool MatchesIndividualProperty(IndividualProperty other) {
            // TODO: Matching is now based mostly based on identical codes.
            // It should also include controlled terminology aliases.
            if (IsAgeProperty && other.IsAgeProperty) {
                return true;
            }
            if (IsSexProperty && other.IsSexProperty) {
                return true;
            }
            return string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);
        }
    }
}
