using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualDayFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual day property values.
    /// </summary>
    public interface IPropertyIndividualDayFilter {

        IndividualProperty IndividualProperty { get; }

        bool IncludeMissingValueRecords { get; set; }

        bool Passes(IndividualDay individualDay);
    }
}
