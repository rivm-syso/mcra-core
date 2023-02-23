using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IndividualProperty {
        public IndividualProperty() {
            CategoricalLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
      
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string PropertyLevelString { get; set; }
        public string PropertyTypeString { get; set; }

        public HashSet<string> CategoricalLevels { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public IndividualPropertyType PropertyType {
            get {
                return IndividualPropertyTypeConverter.FromString(PropertyTypeString);
            }
            set {
                PropertyTypeString = value.ToString();
            }
        }

        public PropertyLevelType PropertyLevel{
            get {
                return PropertyLevelTypeConverter.FromString(PropertyLevelString);
            }
            set {
                PropertyLevelString = value.ToString();
            }
        }
    }
}
