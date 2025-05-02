using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {
    public sealed class AirVentilatoryFlowRateProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            AirVentilatoryFlowRate flowRate
        ) {
            var distributionType = flowRate.DistributionType;

            if (!distributionType.Equals(VentilatoryFlowRateDistributionType.Constant) &&
                flowRate.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for air ventilatory flow rate.";
                throw new Exception(msg);
            }

            var mean = flowRate.Value;
            var cv = flowRate.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                VentilatoryFlowRateDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                VentilatoryFlowRateDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for air ventilatory flow rate.")
            };
            return distribution;
        }
    }
}
