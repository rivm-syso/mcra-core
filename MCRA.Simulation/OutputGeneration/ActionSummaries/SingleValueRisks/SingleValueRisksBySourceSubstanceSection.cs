using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksBySourceSubstanceSection : ActionSummarySectionBase {

        public List<SingleValueRisksBySourceSubstanceRecord> Records { get; set; }

        public void Summarize(
            ICollection<SingleValueRiskCalculationResult> results
        ) {
            Records = results
                .Select(c => {
                    return new SingleValueRisksBySourceSubstanceRecord() {
                        SourceName = c.Source.Name,
                        SourceCode = c.Source.Code,
                        ExposureRoute = c.Source.Route.GetShortDisplayName(),
                        SubstanceName = c.Substance.Name,
                        SubstanceCode = c.Substance.Code,
                        ExposureValue = c.Exposure,
                        HazardCharacterisationValue = c.HazardCharacterisation,
                        ExposureHazardRatio = c.ExposureHazardRatio,
                        HazardExposureRatio = c.HazardExposureRatio,
                        PotencyOrigin = c.Origin.GetShortDisplayName()
                    };
                })
                .OrderByDescending(c => c.ExposureHazardRatio)
                .ToList();
        }
    }
}
