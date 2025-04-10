using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class PopulationCharacteristicDistributionFactory {

        public static Distribution createProbabilityDistribution(
            PopulationCharacteristic populationCharacteristic
        ) {
            var distributionType = populationCharacteristic.DistributionType;

            if (!distributionType.Equals(PopulationCharacteristicDistributionType.Constant) &
                populationCharacteristic.CvVariability == null) {
                var msg = $"CvVariability cannot be missing for distribution {distributionType} for population characteristic.";
                throw new Exception(msg);
            }

            var mean = populationCharacteristic.Value;
            var cv = populationCharacteristic.CvVariability.GetValueOrDefault();

            Distribution distribution = distributionType switch {
                PopulationCharacteristicDistributionType.Normal => NormalDistribution.FromMeanAndCv(mean, cv),
                PopulationCharacteristicDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                PopulationCharacteristicDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                PopulationCharacteristicDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for population characteristic.")
            };
            return distribution;
        }
    }
}
