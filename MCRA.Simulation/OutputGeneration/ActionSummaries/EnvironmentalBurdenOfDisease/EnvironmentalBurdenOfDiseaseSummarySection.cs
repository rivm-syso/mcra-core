using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public List<EnvironmentalBurdenOfDiseaseSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Records = environmentalBurdenOfDiseases
                .GroupBy(r => (r.BodIndicator, r.ExposureResponseFunction))
                .Select(g => new EnvironmentalBurdenOfDiseaseSummaryRecord {
                    BodIndicator = g.Key.BodIndicator.GetShortDisplayName(),
                    ErfCode = g.Key.ExposureResponseFunction.Code,
                    ErfName = g.Key.ExposureResponseFunction.Name,
                    TotalAttributableBod = g.Sum(r => r.AttributableBod)
                })
                .ToList();
        }
    }
}


