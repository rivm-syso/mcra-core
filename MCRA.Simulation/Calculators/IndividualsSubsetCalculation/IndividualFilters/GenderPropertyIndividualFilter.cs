using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public class GenderPropertyIndividualFilter : CategoricalPropertyIndividualFilter {

        public GenderPropertyIndividualFilter(
            IndividualProperty individualProperty,
            ICollection<string> subsetValues
        ) : base(individualProperty, subsetValues) {
            var genders = new HashSet<GenderType>();
            foreach (var value in subsetValues) {
                var gender = GenderTypeConverter.FromString(value);
                genders.Add(gender);
            }

            // Set the accepted values to include the gender aliases of the specified genders
            AcceptedValues = genders
                .SelectMany(r => GenderTypeConverter.UnitDefinition.GetUnitValueDefinition(r).Aliases)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
