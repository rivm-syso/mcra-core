using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureResponseResultRecord> ExposureResponseResults { get; set; }

        public BaselineBodIndicator BaselineBodIndicator { get; set; }

        public double PopulationSize { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureResponseResultRecord> exposureResponseResults = null,
            BaselineBodIndicator baselineBodIndicator = null,
            double populationSize = double.NaN
        ) {
            ExposureResponseResults = exposureResponseResults;
            BaselineBodIndicator = baselineBodIndicator;
            PopulationSize = populationSize;
        }

        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute() {
            var result = ExposureResponseResults
                .Select(compute)
                .ToList();
            var sum = result.Sum(c => c.AttributableBod);
            var cumulative = 0d;
            var sumExposed = result.Sum(c => c.AttributableBod / PopulationSize / c.ExposurePercentileBin.Percentage * 100);
            var cumulativeExposed = 0d;
            foreach (var record in result) {
                cumulative += record.AttributableBod;
                cumulativeExposed += record.AttributableBod / PopulationSize  / record.ExposurePercentileBin.Percentage * 100;
                record.CumulativeAttributableBod = cumulative / sum * 100;
                record.CumulativeStandardizedExposedAttributableBod = cumulativeExposed / sumExposed * 100;
            }
            return result;
        }

        private EnvironmentalBurdenOfDiseaseResultRecord compute(
            ExposureResponseResultRecord exposureResponseResultRecord
        ) {
            var result = new EnvironmentalBurdenOfDiseaseResultRecord {
                BaselineBodIndicator = BaselineBodIndicator,
                ExposureBinId = exposureResponseResultRecord.ExposureBinId,
                ExposureBin = exposureResponseResultRecord.ExposureInterval,
                ExposurePercentileBin = exposureResponseResultRecord.PercentileInterval,
                ErfDoseUnit = exposureResponseResultRecord.ExposureResponseFunction.DoseUnit,
                ResponseValue = exposureResponseResultRecord.PercentileSpecificRisk,
                TargetUnit = exposureResponseResultRecord?.TargetUnit
            };
            result.AttributableFraction = (result.ResponseValue - 1) / result.ResponseValue;
            result.TotalBod = BaselineBodIndicator.Value
                * exposureResponseResultRecord.PercentileInterval.Percentage / 100;
            result.AttributableBod = result.TotalBod * result.AttributableFraction;
            result.ExposureResponseResultRecord = exposureResponseResultRecord;
            return result;
        }
    }
}
