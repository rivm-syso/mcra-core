using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureEffectResultRecord> ExposureEffectResults { get; set; }

        public BaselineBodIndicator BaselineBodIndicator { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureEffectResultRecord> exposureEffectResults = null,
            BaselineBodIndicator baselineBodIndicator = null) {
            ExposureEffectResults = exposureEffectResults;
            BaselineBodIndicator = baselineBodIndicator;
        }

        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute() {
            var result = ExposureEffectResults
                .Select(compute)
                .ToList();
            return result;
        }

        private EnvironmentalBurdenOfDiseaseResultRecord compute(
            ExposureEffectResultRecord exposureEffectResultRecord
        ) {
            var result = new EnvironmentalBurdenOfDiseaseResultRecord();
            result.BodIndicator = BaselineBodIndicator.BodIndicator;
            result.ExposureBin = exposureEffectResultRecord.PercentileInterval;
            result.Unit = exposureEffectResultRecord.ExposureEffectFunction.DoseUnit.GetShortDisplayName();
            result.Ratio = exposureEffectResultRecord.PercentileSpecificRisk;
            result.AttributableFraction = (result.Ratio - 1) / result.Ratio;
            result.TotalBod = BaselineBodIndicator.Value * (exposureEffectResultRecord.PercentileInterval.Upper -
                exposureEffectResultRecord.PercentileInterval.Lower) / 100;
            result.AttributableBod = result.TotalBod * result.AttributableFraction;
            result.ExposureEffectResultRecord = exposureEffectResultRecord;

            return result;
        }
    }
}
