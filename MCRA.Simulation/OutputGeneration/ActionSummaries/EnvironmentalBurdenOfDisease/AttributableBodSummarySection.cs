using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            Records = environmentalBurdenOfDiseases
                .Select((s, ix) => new AttributableBodSummaryRecord {
                    ExposureBinId = s.ExposureBinId,
                    Population = s.BaselineBodIndicator.Population.Name,
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
                    CumulativeAttributableBods = [],
                    CumulativeAttributableBod = s.CumulativeAttributableBod,
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
                record.CumulativeAttributableBods.Add(item.CumulativeAttributableBod);
                record.Exposures.Add(item.Exposure);
            }
        }
    }
}
