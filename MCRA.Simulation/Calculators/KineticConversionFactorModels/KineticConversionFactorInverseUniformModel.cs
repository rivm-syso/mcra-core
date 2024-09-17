using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public sealed class KineticConversionFactorInverseUniformModel(
        KineticConversionFactor conversion,
        bool useSubgroups
    ) : KineticConversionFactorDistributionModel<InverseUniformDistribution>(conversion, useSubgroups) {
        protected override InverseUniformDistribution getDistributionFromNominalAndUpper(double factor, double upper) {
            var distribution = InverseUniformDistribution.FromMedianAndUpper(factor, upper);
            return distribution;
        }
    }
}
