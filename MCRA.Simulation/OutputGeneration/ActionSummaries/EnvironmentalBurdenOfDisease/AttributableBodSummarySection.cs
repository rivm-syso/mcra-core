using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            Records = environmentalBurdenOfDiseases
                .Select(s => new AttributableBodSummaryRecord {
                    BodIndicator = s.BodIndicator.GetShortDisplayName(),
                    ExposureResponseFunctionCode = s.ExposureResponseFunction.Code,
                    ExposureBin = s.ExposureBin.ToString(),
                    Exposure = s.Exposure,
                    Exposures = [],
                    Unit = s.Unit,
                    Ratio = s.Ratio,
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
                var record = Records.FirstOrDefault(c => c.ExposureBin == item.ExposureBin.ToString()
                    && c.ExposureResponseFunctionCode == item.ExposureResponseFunction.Code
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
