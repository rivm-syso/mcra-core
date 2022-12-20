using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public sealed class MarginalOccurrencePattern : OccurrencePattern {

        public double AnalyticalScopeCount { get; set; }

        public double PositiveFindingsCount { get; set; }

        public bool? AuthorisedUse { get; set; }

    }
}
