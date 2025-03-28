using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureResponseResultRecord> ExposureResponseResults { get; set; }

        public BaselineBodIndicator BaselineBodIndicator { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureResponseResultRecord> exposureResponseResults = null,
            BaselineBodIndicator baselineBodIndicator = null
        ) {
            ExposureResponseResults = exposureResponseResults;
            BaselineBodIndicator = baselineBodIndicator;
        }

        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute() {
            var result = ExposureResponseResults
                .Select(compute)
                .ToList();
            var sum = result.Sum(c => c.AttributableBod);
            var cumulative = 0d;
            foreach (var record in result) {
                cumulative += record.AttributableBod;
                record.CumulativeAttributableBod = cumulative / sum * 100;
            }
            return result;
        }

        private EnvironmentalBurdenOfDiseaseResultRecord compute(
            ExposureResponseResultRecord exposureResponesResultRecord
        ) {
            var result = new EnvironmentalBurdenOfDiseaseResultRecord {
                BodIndicator = BaselineBodIndicator.BodIndicator,
                ExposureBinId = exposureResponesResultRecord.ExposureBinId,
                ExposureBin = exposureResponesResultRecord.ExposureInterval,
                ExposurePercentileBin = exposureResponesResultRecord.PercentileInterval,
                ErfDoseUnit = exposureResponesResultRecord.ExposureResponseFunction.DoseUnit,
                ResponseValue = exposureResponesResultRecord.PercentileSpecificRisk,
                TargetUnit = exposureResponesResultRecord?.TargetUnit
            };
            result.AttributableFraction = (result.ResponseValue - 1) / result.ResponseValue;
            result.TotalBod = BaselineBodIndicator.Value 
                * exposureResponesResultRecord.PercentileInterval.Percentage / 100;
            result.AttributableBod = result.TotalBod * result.AttributableFraction;
            result.ExposureResponseResultRecord = exposureResponesResultRecord;
            return result;
        }
    }
}
