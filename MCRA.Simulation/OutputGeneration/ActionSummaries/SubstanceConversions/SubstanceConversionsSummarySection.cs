using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstanceConversionsSummarySection : ActionSummarySectionBase {

        public int SubstanceConversionsCount { get; set; }

        public int DeterministicSubstanceConversionsCount { get; set; }

        public void Summarize(
            ICollection<SubstanceConversion> substanceConversions,
            ICollection<DeterministicSubstanceConversionFactor> deterministicSubstanceConversionFactors
        ) {
            var groupedResidueDefinitions = substanceConversions.GroupBy(r => r.MeasuredSubstance);
            SubstanceConversionsCount = substanceConversions?.Count ?? 0;
            DeterministicSubstanceConversionsCount = deterministicSubstanceConversionFactors?.Count ?? 0;
        }
    }
}
