using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public interface ISingleValueDietaryExposureCalculator {

        SingleValueDietaryExposuresCalculationMethod CalculationMethod { get; }

        ICollection<ISingleValueDietaryExposure> Compute(
            Population population,
            ICollection<Compound> substances,
            ICollection<SingleValueConsumptionModel> singleValueConsumptionModels,
            IDictionary<(Food, Compound), SingleValueConcentrationModel> singleValueConcentrationModels,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            ConsumptionIntakeUnit consumptionIntakeUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit consumptionBodyWeightUnit,
            TargetUnit targetUnit
        );
    }
}
