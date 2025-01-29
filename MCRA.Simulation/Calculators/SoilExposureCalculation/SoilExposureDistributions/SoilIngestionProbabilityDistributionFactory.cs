using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public sealed class SoilIngestionProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            SoilIngestion soilIngestion
        ) {
            var distributionType = soilIngestion.DistributionType;

            if (!distributionType.Equals(SoilIngestionDistributionType.Constant) &
                soilIngestion.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for soil ingestion.";
                throw new Exception(msg);
            }

            var mean = soilIngestion.Value;
            var cv = soilIngestion.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                SoilIngestionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                SoilIngestionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                SoilIngestionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for soil ingestion.")
            };
            return distribution;
        }
    }
}
