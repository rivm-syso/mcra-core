using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public List<EnvironmentalBurdenOfDiseaseSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Records = environmentalBurdenOfDiseases
                .GroupBy(r => (r.BaselineBodIndicator, r.ExposureResponseFunction))
                .Select(g => new EnvironmentalBurdenOfDiseaseSummaryRecord {
                    Population = g.Key.BaselineBodIndicator.Population.Name,
                    BodIndicator = g.Key.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                    ErfCode = g.Key.ExposureResponseFunction.Code,
                    ErfName = g.Key.ExposureResponseFunction.Name,
                    TotalAttributableBod = g.Sum(r => r.AttributableBod)
                })
                .ToList();
        }
    }
}


