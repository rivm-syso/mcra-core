using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public sealed class KineticConversionFactorLogNormalModel(
        KineticConversionFactor conversion,
        bool useSubgroups
    ) : KineticConversionFactorDistributionModel<LogNormalDistribution>(conversion, useSubgroups) {
        protected override LogNormalDistribution getDistributionFromNominalAndUpper(double factor, double upper) {
            var distribution = LogNormalDistribution.FromMeanAndUpper(factor, upper);
            return distribution;
        }
    }
}
