using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustAvailabilityFractionProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            DustAvailabilityFraction dustAvailabilityFraction
        ) {
            var distributionType = dustAvailabilityFraction.DistributionType;

            if (!distributionType.Equals(DustAvailabilityFractionDistributionType.Constant) &
                dustAvailabilityFraction.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for dust availability fraction.";
                throw new Exception(msg);
            }

            var mean = dustAvailabilityFraction.Value;
            var cv = dustAvailabilityFraction.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                DustAvailabilityFractionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                DustAvailabilityFractionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                DustAvailabilityFractionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for dust availability fraction.")
            };
            return distribution;
        }
    }
}
