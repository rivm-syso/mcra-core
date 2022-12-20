using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public interface IPropertyIndividualFilter : IFilter<Individual> {

        IndividualProperty IndividualProperty { get; }

        bool IncludeMissingValueRecords { get; set; }

    }
}
