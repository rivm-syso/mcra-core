using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualDayFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual day property values.
    /// </summary>
    public interface IPropertyIndividualDayFilter : IFilter<IndividualDay> {

        IndividualProperty IndividualProperty { get; }

        bool IncludeMissingValueRecords { get; set; }

    }
}
