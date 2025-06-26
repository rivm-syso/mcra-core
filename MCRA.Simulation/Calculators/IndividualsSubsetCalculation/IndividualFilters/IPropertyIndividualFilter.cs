using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public interface IPropertyIndividualFilter {

        IndividualProperty IndividualProperty { get; }

        bool IncludeMissingValueRecords { get; set; }

        bool Passes(Individual individual);

    }
}
