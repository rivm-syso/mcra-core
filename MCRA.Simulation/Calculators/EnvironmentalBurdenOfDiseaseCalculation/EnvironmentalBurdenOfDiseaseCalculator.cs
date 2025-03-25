using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureResponseResultRecord> ExposureResponseResults { get; set; }

        public BaselineBodIndicator BaselineBodIndicator { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureResponseResultRecord> exposureResponseResults = null,
            BaselineBodIndicator baselineBodIndicator = null) {
            ExposureResponseResults = exposureResponseResults;
            BaselineBodIndicator = baselineBodIndicator;
        }

        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute() {
            var result = ExposureResponseResults
                .Select(compute)
                .ToList();
            return result;
        }

        private EnvironmentalBurdenOfDiseaseResultRecord compute(
            ExposureResponseResultRecord exposureResponesResultRecord
        ) {
            var result = new EnvironmentalBurdenOfDiseaseResultRecord();
            result.BodIndicator = BaselineBodIndicator.BodIndicator;
            result.ExposureBin = exposureResponesResultRecord.PercentileInterval;
            result.Unit = exposureResponesResultRecord.ExposureResponseFunction.DoseUnit.GetShortDisplayName();
            result.Ratio = exposureResponesResultRecord.PercentileSpecificRisk;
            result.AttributableFraction = (result.Ratio - 1) / result.Ratio;
            result.TotalBod = BaselineBodIndicator.Value * (exposureResponesResultRecord.PercentileInterval.Upper -
                exposureResponesResultRecord.PercentileInterval.Lower) / 100;
            result.AttributableBod = result.TotalBod * result.AttributableFraction;
            result.ExposureResponseResultRecord = exposureResponesResultRecord;

            return result;
        }
    }
}
