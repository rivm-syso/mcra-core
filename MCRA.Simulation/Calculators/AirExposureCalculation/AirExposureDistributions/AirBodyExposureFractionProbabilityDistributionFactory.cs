using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class AirBodyExposureFractionProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            AirBodyExposureFraction bodyExposureFraction
        ) {
            var distributionType = bodyExposureFraction.DistributionType;

            if (!distributionType.Equals(AirBodyExposureFractionDistributionType.Constant) &&
                bodyExposureFraction.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for air body exposure fraction.";
                throw new Exception(msg);
            }

            var mean = bodyExposureFraction.Value;
            var cv = bodyExposureFraction.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                AirBodyExposureFractionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                AirBodyExposureFractionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                AirBodyExposureFractionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for air body exposure fraction.")
            };
            return distribution;
        }
    }
}
