using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            Population population
        ) {
            Records = environmentalBurdenOfDiseases
                .Select((s, ix) => new AttributableBodSummaryRecord {
                    ExposureBinId = s.ExposureBinId,
                    PopulationCode = s.BaselineBodIndicator.Population.Code,
                    PopulationName = s.BaselineBodIndicator.Population.Name,
                    PopulationSize = population.Size > 0 ? population.Size : double.NaN,
                    BodIndicator = s.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                    ExposureResponseFunctionCode = s.ExposureResponseFunction.Code,
                    BinPercentage = s.ExposurePercentileBin.Percentage,
                    ExposurePercentileBin = s.ExposurePercentileBin.ToString(),
                    ExposureBin = s.ExposureBin.ToString(),
                    Exposure = s.Exposure,
                    Exposures = [],
                    TargetUnit = s.TargetUnit.GetShortDisplayName(),
                    ResponseValue = s.ResponseValue,
                    AttributableFraction = s.AttributableFraction,
                    TotalBod = s.TotalBod,
                    AttributableBod = s.AttributableBod,
                    AttributableBods = [],
                    StandardizedAttributableBods = [],
                    BinPercentages = [],
                    TotalBods = [],
                    CumulativeAttributableBods = [],
                    CumulativeAttributableBod = s.CumulativeAttributableBod,
                    CumulativeStandardizedExposedAttributableBods = [],
                    CumulativeStandardizedExposedAttributableBod = s.CumulativeStandardizedExposedAttributableBod,
                })
                .ToList();
        }

        public void SummarizeUncertainty(
           List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
           double lowerBound,
           double upperBound
        ) {
            var results = new List<AttributableBodSummaryRecord>();
            foreach (var item in environmentalBurdenOfDiseases) {
                var record = Records
                    .FirstOrDefault(c => c.ExposureResponseFunctionCode == item.ExposureResponseFunction.Code
                        && c.ExposureBinId == item.ExposureBinId
                );
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                record.AttributableBods.Add(item.AttributableBod);
                record.StandardizedAttributableBods.Add(item.AttributableBod / item.ExposurePercentileBin.Percentage);
                record.BinPercentages.Add(item.ExposurePercentileBin.Percentage);
                record.TotalBods.Add(item.TotalBod);
                record.CumulativeAttributableBods.Add(item.CumulativeAttributableBod);
                record.CumulativeStandardizedExposedAttributableBods.Add(item.CumulativeStandardizedExposedAttributableBod);
                record.Exposures.Add(item.Exposure);
            }
        }
    }
}
