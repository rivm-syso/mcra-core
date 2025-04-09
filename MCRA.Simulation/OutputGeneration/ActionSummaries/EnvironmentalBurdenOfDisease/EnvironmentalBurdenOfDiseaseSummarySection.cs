using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public List<EnvironmentalBurdenOfDiseaseSummaryRecord> Records { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            Population population
        ) {
            Records = environmentalBurdenOfDiseases
                .GroupBy(r => (r.BaselineBodIndicator, r.ExposureResponseFunction))
                .Select(g => {
                    var totalAttributableBod = g.Sum(r => r.AttributableBod);
                    return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                        BodIndicator = g.Key.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                        PopulationCode = g.Key.BaselineBodIndicator.Population.Code,
                        PopulationName = g.Key.BaselineBodIndicator.Population.Name,
                        ErfCode = g.Key.ExposureResponseFunction.Code,
                        ErfName = g.Key.ExposureResponseFunction.Name,
                        TotalAttributableBod = totalAttributableBod,
                        StandardizedTotalAttributableBod = totalAttributableBod / population.Size * 1e5
                    };
                })
                .ToList();
        }
    }
}


