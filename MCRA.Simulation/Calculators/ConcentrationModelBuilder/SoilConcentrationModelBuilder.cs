using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {

    /// <summary>
    /// Builder class for Soil concentration models.
    /// </summary>
    public class SoilConcentrationModelBuilder : ConcentrationModelBuilderBase{


        protected override ConcentrationModel getModel<T>(T concentrationDistribution) {
            var distribution = concentrationDistribution as SoilConcentrationDistribution;
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                SoilConcentrationDistributionType.Constant => new CMConstant(),
                SoilConcentrationDistributionType.LogNormal => new CMSummaryStatistics(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for soil distributions."),
            };
            return concentrationModel;
        }

        protected override double getAlignmentFactor<T>(ConcentrationUnit targetConcentrationUnit, T concentrationDistribution) {
            var distribution = concentrationDistribution as SoilConcentrationDistribution;
            var alignmentFactor = distribution.Unit
                .GetConcentrationAlignmentFactor(targetConcentrationUnit, distribution.Substance.MolecularMass);
            return alignmentFactor;
        }
    }
}
