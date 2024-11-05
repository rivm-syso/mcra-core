using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustAdherenceAmountProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            DustAdherenceAmount dustAdherenceAmount
        ) {
            var distributionType = dustAdherenceAmount.DistributionType;

            if (!distributionType.Equals(DustAdherenceAmountDistributionType.Constant) &
                dustAdherenceAmount.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for dust adherence amount.";
                throw new Exception(msg);
            }

            var mean = dustAdherenceAmount.Value;
            var cv = dustAdherenceAmount.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                DustAdherenceAmountDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                DustAdherenceAmountDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                DustAdherenceAmountDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for dust adherence amount.")
            };
            return distribution;
        }
    }
}
