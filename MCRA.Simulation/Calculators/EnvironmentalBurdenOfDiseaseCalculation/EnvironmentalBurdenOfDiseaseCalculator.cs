using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureEffectResultRecord> ExposureEffectResults { get; set; }

        public double TotalBurdenOfDisease { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureEffectResultRecord> exposureEffectResults = null,
            double totalBurdenOfDisease = 0.0) {
            ExposureEffectResults = exposureEffectResults;
            TotalBurdenOfDisease = totalBurdenOfDisease;
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
            result.ExposureBin = exposureEffectResultRecord.PercentileInterval;
            result.Unit = exposureEffectResultRecord.ExposureEffectFunction.DoseUnit.GetShortDisplayName();
            result.Ratio = exposureEffectResultRecord.PercentileSpecificRisk;
            result.AttributableFraction = (result.Ratio - 1) / result.Ratio;
            result.TotalBod = TotalBurdenOfDisease * (exposureEffectResultRecord.PercentileInterval.Upper -
                exposureEffectResultRecord.PercentileInterval.Lower) / 100;
            result.AttributableBod = result.TotalBod * result.AttributableFraction;
            result.ExposureEffectResultRecord = exposureEffectResultRecord;

            return result;
        }
    }
}
