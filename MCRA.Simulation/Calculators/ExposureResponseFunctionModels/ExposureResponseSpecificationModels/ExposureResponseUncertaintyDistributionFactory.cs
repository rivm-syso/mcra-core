using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {
    public class ExposureResponseUncertaintyDistributionFactory {

        public static Distribution GetDistribution(
            ExposureResponseSpecificationDistributionType distributionType,
            Func<double> getNominal,
            Func<double> getLower,
            Func<double> getUpper
        ) {
            return distributionType switch {
                ExposureResponseSpecificationDistributionType.Normal => NormalDistribution.FromMeanAndUpper(getNominal(), getUpper()),
                ExposureResponseSpecificationDistributionType.LogNormal => LogNormalDistribution.FromMeanAndUpper(getNominal(), getUpper()),
                ExposureResponseSpecificationDistributionType.Triangular => TriangularDistribution.FromModeLowerandUpper(getNominal(), getLower(), getUpper()),
                ExposureResponseSpecificationDistributionType.Uniform => UniformDistribution.FromMeanAndUpper(getNominal(), getUpper()),
                ExposureResponseSpecificationDistributionType.Constant => null,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
