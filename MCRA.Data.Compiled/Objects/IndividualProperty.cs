using MCRA.General;

namespace MCRA.Data.Compiled.Objects {

    public sealed class IndividualProperty : StrongEntity {

        /// <summary>
        /// Reserved names for age property.
        /// </summary>
        public static HashSet<string> AgePropertyAliases { get; } = new(StringComparer.OrdinalIgnoreCase) {
            "Age"
        };

        /// <summary>
        /// At present, sex and gender are used interchangeably when making selections on individual properties.
        /// </summary>
        public static HashSet<string> SexPropertyAliases { get; } = new(StringComparer.OrdinalIgnoreCase) {
            "Sex", "Gender"
        };

        /// <summary>
        /// Reserved names for height property.
        /// </summary>
        public static HashSet<string> HeightPropertyAliases { get; } = new(StringComparer.OrdinalIgnoreCase) {
            "Height"
        };

        /// <summary>
        /// Reserved names for BSA (body surface area) property.
        /// </summary>
        public static HashSet<string> BsaPropertyAliases { get; } = new(StringComparer.OrdinalIgnoreCase) {
            "BSA"
        };

        public IndividualProperty() {
            CategoricalLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public IndividualPropertyType PropertyType { get; set; }

        public PropertyLevelType PropertyLevel { get; set; }

        public HashSet<string> CategoricalLevels { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public bool IsAgeProperty => AgePropertyAliases.Contains(Name);

        public bool IsSexProperty => SexPropertyAliases.Contains(Name);

        public bool IsHeightProperty => HeightPropertyAliases.Contains(Name);

        public bool IsBsaProperty => BsaPropertyAliases.Contains(Name);

        public bool MatchesIndividualProperty(IndividualProperty other) {
            if (IsAgeProperty && other.IsAgeProperty) {
                return true;
            } else if (IsSexProperty && other.IsSexProperty) {
                return true;
            } else if (IsHeightProperty && other.IsHeightProperty) {
                return true;
            } else if (IsBsaProperty && other.IsBsaProperty) {
                return true;
            }
            return string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);
        }

        public static IndividualProperty FromName(string name, bool isNumeric) {
            var propertyType = IndividualPropertyTypeConverter
                .FromString(
                    name,
                    isNumeric ? IndividualPropertyType.Numeric : IndividualPropertyType.Categorical,
                    true
                );
            return new IndividualProperty {
                Code = name,
                Name = name,
                PropertyType = propertyType,
                PropertyLevel = PropertyLevelType.Individual
            };
        }
    }
}
