using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Filters.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public class BooleanPropertyIndividualFilter : CategoricalPropertyIndividualFilter {

        public BooleanPropertyIndividualFilter(
            IndividualProperty individualProperty,
            ICollection<string> subsetValues
        ) : base(individualProperty, subsetValues) {
            var values = new HashSet<BooleanType>();
            foreach (var value in subsetValues) {
                var val = BooleanTypeConverter.FromString(value);
                values.Add(val);
            }

            AcceptedValues = values
                .SelectMany(r => BooleanTypeConverter.UnitDefinition.GetUnitValueDefinition(r).Aliases)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
