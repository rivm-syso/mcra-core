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
                    var attributableBodPerBinList = new List<(int id, List<double>)>();
                    var totalAttributableBod = g.Sum(r => r.AttributableBod);
                    var bods = g.Select(c => (c.ExposureBinId, c.AttributableBod)).ToList();
                    foreach (var bod in bods) {
                        attributableBodPerBinList.Add((bod.ExposureBinId, new List<double>()));
                    }
                    return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                        PopulationSize = population.Size > 0 ? population.Size : double.NaN,
                        BodIndicator = g.Key.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                        PopulationCode = g.Key.BaselineBodIndicator.Population.Code,
                        PopulationName = g.Key.BaselineBodIndicator.Population.Name,
                        ErfCode = g.Key.ExposureResponseFunction.Code,
                        ErfName = g.Key.ExposureResponseFunction.Name,
                        TotalAttributableBod = totalAttributableBod,
                        AttributableBodPerBin = [],
                        AttributableBodPerBinList = attributableBodPerBinList,
                        TotalAttributableBods = [],
                    };
                })
                .ToList();
        }

        public void SummarizeUncertainty(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            Population population,
            double lowerBound,
            double upperBound
        ) {
            var records = environmentalBurdenOfDiseases
                .GroupBy(r => (r.BaselineBodIndicator, r.ExposureResponseFunction))
                .Select(g => {
                    var totalAttributableBod = g.Sum(r => r.AttributableBod);
                    var bods = g.Select(c => (c.ExposureBinId, c.AttributableBod)).ToList();
                    return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                        BodIndicator = g.Key.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                        PopulationCode = g.Key.BaselineBodIndicator.Population.Code,
                        PopulationName = g.Key.BaselineBodIndicator.Population.Name,
                        ErfCode = g.Key.ExposureResponseFunction.Code,
                        ErfName = g.Key.ExposureResponseFunction.Name,
                        TotalAttributableBod = totalAttributableBod,
                        AttributableBodPerBin = bods,
                    };
                })
                .ToList();

            foreach (var record in Records) {
                var result = records.Where(r => r.ErfCode == record.ErfCode).First();
                foreach (var bin in result.AttributableBodPerBin) {
                    record.UncertaintyLowerBound = lowerBound;
                    record.UncertaintyUpperBound = upperBound;
                    record.AttributableBodPerBinList.First(c => c.Id == bin.Id).Bods.Add(bin.Bod);
                }
                record.TotalAttributableBods.Add(result.TotalAttributableBod);
            }
        }
    }
}


