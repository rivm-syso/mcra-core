using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {
    /// <summary>
    /// Builder class for dust concentration models.
    /// </summary>
    public class DustConcentrationModelBuilder : ConcentrationModelBuilderBase {

        protected override ConcentrationModel getModel<T>(T concentrationDistribution) {
            var distribution = concentrationDistribution as DustConcentrationDistribution;
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                DustConcentrationDistributionType.Constant => new CMConstant(),
                DustConcentrationDistributionType.LogNormal => new CMLogNormal(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for dust distributions."),
            };
            return concentrationModel;
        }

        protected override double getAlignmentFactor<T>(
            ConcentrationUnitSpec targetUnit,
            T concentrationDistribution
        ) {
            if (targetUnit is not StandardConcentrationUnitSpec stdUnit) {
                throw new InvalidOperationException(
                    $"Expected a standard concentration unit but received {targetUnit?.GetType().Name}."
                );
            }
            var distribution = concentrationDistribution as DustConcentrationDistribution;
            var alignmentFactor = distribution.Unit
                .GetConcentrationAlignmentFactor(stdUnit.Unit, distribution.Substance.MolecularMass);
            return alignmentFactor;
        }
    }
}
