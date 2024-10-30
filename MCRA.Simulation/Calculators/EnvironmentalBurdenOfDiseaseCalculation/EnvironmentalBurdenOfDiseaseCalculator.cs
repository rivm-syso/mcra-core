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
            result.PercentileInterval = exposureEffectResultRecord.PercentileInterval;
            result.Unit = exposureEffectResultRecord.ExposureEffectFunction.DoseUnit.GetShortDisplayName();
            result.PercentileSpecificOr = exposureEffectResultRecord.PercentileSpecificRisk;
            result.PercentileSpecificAf = (result.PercentileSpecificOr - 1) / result.PercentileSpecificOr;
            result.AbsoluteBod = TotalBurdenOfDisease * (exposureEffectResultRecord.PercentileInterval.Upper -
                exposureEffectResultRecord.PercentileInterval.Lower) / 100;
            result.AttributableEbd = result.AbsoluteBod * result.PercentileSpecificAf;
            result.ExposureEffectResultRecord = exposureEffectResultRecord;

            return result;
        }
    }
}
