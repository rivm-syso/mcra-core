using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustBodyExposureFractionProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            DustBodyExposureFraction dustBodyExposureFraction
        ) {
            var distributionType = dustBodyExposureFraction.DistributionType;

            if (!distributionType.Equals(DustBodyExposureFractionDistributionType.Constant) &&
                dustBodyExposureFraction.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for dust body exposure fraction.";
                throw new Exception(msg);
            }

            var mean = dustBodyExposureFraction.Value;
            var cv = dustBodyExposureFraction.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                DustBodyExposureFractionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                DustBodyExposureFractionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                DustBodyExposureFractionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for dust body exposure fraction.")
            };
            return distribution;
        }
    }
}
