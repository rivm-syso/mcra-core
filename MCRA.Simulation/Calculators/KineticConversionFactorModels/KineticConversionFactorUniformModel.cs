using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public sealed class KineticConversionFactorUniformModel(
        KineticConversionFactor conversion,
        bool useSubgroups
    ) : KineticConversionFactorDistributionModel<UniformDistribution>(conversion, useSubgroups) {
        protected override UniformDistribution getDistributionFromNominalAndUpper(double factor, double upper) {
            var distribution = UniformDistribution.FromMeanAndUpper(factor, upper);
            return distribution;
        }
    }
}
