using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {

    /// <summary>
    /// Builder class for dust concentration models.
    /// </summary>
    public class AirConcentrationModelBuilder : ConcentrationModelBuilderBase {

        protected override ConcentrationModel getModel<T>(T concentrationDistribution) {
            var distribution = concentrationDistribution as AirConcentrationDistribution;
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                AirConcentrationDistributionType.Constant => new CMConstant(),
                AirConcentrationDistributionType.LogNormal => new CMSummaryStatistics(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for air distributions."),
            };
            return concentrationModel;
        }

        protected override double getAlignmentFactor<T>(AirConcentrationUnit targetConcentrationUnit, T concentrationDistribution) {
            var distribution = concentrationDistribution as AirConcentrationDistribution;
            var alignmentFactor = distribution.Unit
                .GetConcentrationAlignmentFactor(targetConcentrationUnit, distribution.Substance.MolecularMass);
            return alignmentFactor;
        }
    }
}
