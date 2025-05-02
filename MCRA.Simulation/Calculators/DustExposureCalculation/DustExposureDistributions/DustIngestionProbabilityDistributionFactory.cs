using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIngestionProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            DustIngestion dustIngestion
        ) {
            var distributionType = dustIngestion.DistributionType;

            if (!distributionType.Equals(DustIngestionDistributionType.Constant) &&
                dustIngestion.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for dust ingestion.";
                throw new Exception(msg);
            }

            var mean = dustIngestion.Value;
            var cv = dustIngestion.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                DustIngestionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                DustIngestionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                DustIngestionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for dust ingestion.")
            };
            return distribution;
        }
    }
}
