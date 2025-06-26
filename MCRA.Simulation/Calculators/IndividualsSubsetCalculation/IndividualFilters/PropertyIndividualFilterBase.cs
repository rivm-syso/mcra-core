using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public abstract class PropertyIndividualFilterBase : IPropertyIndividualFilter {

        public IndividualProperty IndividualProperty { get; protected set; }

        public bool IncludeMissingValueRecords { get; set; } = false;

        public PropertyIndividualFilterBase(IndividualProperty individualProperty) {
            IndividualProperty = individualProperty;
        }

        public abstract bool Passes(Individual item);
    }
}
