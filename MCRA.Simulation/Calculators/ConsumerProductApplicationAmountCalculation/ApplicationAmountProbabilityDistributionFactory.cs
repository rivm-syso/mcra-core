using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductAplicationAmountCalculation {
    public sealed class ApplicationAmountProbabilityDistributionFactory {

        public static Distribution createProbabilityDistribution(
            ConsumerProductApplicationAmount applicationAmount
        ) {
            var distributionType = applicationAmount.DistributionType;

            if (!distributionType.Equals(ApplicationAmountDistributionType.Constant)
                && applicationAmount.CvVariability == null
            ) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} of consumer product {applicationAmount.Product}.";
                throw new Exception(msg);
            }

            var mean = applicationAmount.Amount;
            var cv = applicationAmount.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                ApplicationAmountDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                ApplicationAmountDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for consumer product {applicationAmount.Product}.")
            };
            return distribution;
        }
    }
}
