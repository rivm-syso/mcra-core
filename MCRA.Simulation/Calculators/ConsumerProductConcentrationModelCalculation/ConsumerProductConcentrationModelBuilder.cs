using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConsumerProductConcentrationModelCalculation {

    /// <summary>
    /// Builder class for consumer product concentration models.
    /// </summary>
    public class ConsumerProductConcentrationModelBuilder : ConcentrationModelBuilderBase {

        protected override ConcentrationModel getModel<T>(T concentrationDistribution) {
            var distribution = concentrationDistribution as ConsumerProductConcentrationDistribution;
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                ConsumerProductConcentrationDistributionType.Constant => new CMConstant(),
                ConsumerProductConcentrationDistributionType.LogNormal => new CMSummaryStatistics(),
                ConsumerProductConcentrationDistributionType.Uniform => new CMSummaryStatistics(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for consumer product distributions."),
            };
            return concentrationModel;
        }

        protected override double getAlignmentFactor<T>(
            ConcentrationUnitSpec targetUnit,
            T concentrationDistribution
        ) {
            if (targetUnit is not ConsumerProductConcentrationUnitSpec cpUnit) {
                throw new InvalidOperationException(
                    $"Expected a consumer product concentration unit but received {targetUnit?.GetType().Name}."
                );
            }
            var distribution = concentrationDistribution as ConsumerProductConcentrationDistribution;
            var alignmentFactor = distribution.Unit
                .GetConcentrationAlignmentFactor(cpUnit.Unit, distribution.Substance.MolecularMass);
            return alignmentFactor;
        }
    }
}
