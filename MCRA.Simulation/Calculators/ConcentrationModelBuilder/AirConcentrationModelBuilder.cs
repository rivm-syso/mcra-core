using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {

    /// <summary>
    /// Builder class for air concentration models.
    /// </summary>
    public class AirConcentrationModelBuilder : ConcentrationModelBuilderBase {

        protected override ConcentrationModel getModel<T>(T concentrationDistribution) {
            var distribution = concentrationDistribution as AirConcentrationDistribution;
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                AirConcentrationDistributionType.Constant => new CMConstant(),
                AirConcentrationDistributionType.LogNormal => new CMLogNormal(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for air distributions."),
            };
            return concentrationModel;
        }

        protected override double getAlignmentFactor<T>(
            ConcentrationUnitSpec targetUnit,
            T concentrationDistribution
        ) {
            if (targetUnit is not AirConcentrationUnitSpec airUnit) {
                throw new InvalidOperationException(
                    $"Expected an air concentration unit but received {targetUnit?.GetType().Name}."
                );
            }
            var distribution = concentrationDistribution as AirConcentrationDistribution;
            var alignmentFactor = distribution.Unit
                .GetConcentrationAlignmentFactor(airUnit.Unit, distribution.Substance.MolecularMass);
            return alignmentFactor;
        }
    }
}
