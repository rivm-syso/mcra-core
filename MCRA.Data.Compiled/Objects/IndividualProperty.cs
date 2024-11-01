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

        public bool IsAgeProperty() {
            return string.Equals(Name, "Age", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSexProperty() {
            return string.Equals(Name, "Sex", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Name, "Gender", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsHeightProperty() {
            return string.Equals(Name, "Height", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsBsaProperty() {
            return string.Equals(Name, "BSA", StringComparison.OrdinalIgnoreCase);
        }
    }
}
