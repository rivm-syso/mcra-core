using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    public class IntakeCalculatorFactory {

        private readonly IIntakeCalculatorFactorySettings _settings;

        public IntakeCalculatorFactory(IIntakeCalculatorFactorySettings settings) {
            _settings = settings;
        }

        public DietaryExposureCalculatorBase Create(
            ProcessingFactorModelCollection processingFactorModels,
            IDictionary<(Food, Compound), double> tdsReductionFactors,
            IResidueGenerator residueGenerator,
            UnitVariabilityCalculator unitVariabilityCalculator,
            IIndividualDayIntakePruner individualDayIntakePruner,
            ICollection<ConsumptionsByModelledFood> consumptionsPerFoodAsMeasured,
            ICollection<Compound> activeSubstances,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels
        ) {
            var consumptionsByFoodsAsMeasured = ConsumptionsByModelledFoodCalculator
                .CreateIndividualDayLookUp(consumptionsPerFoodAsMeasured);

            if (_settings.ExposureType == ExposureType.Acute) {
                return new AcuteDietaryExposureCalculator(
                    activeSubstances,
                    consumptionsByFoodsAsMeasured,
                    processingFactorModels,
                    individualDayIntakePruner,
                    residueGenerator,
                    unitVariabilityCalculator,
                    consumptionsPerFoodAsMeasured,
                    _settings.NumberOfMonteCarloIterations,
                    _settings.IsSampleBased,
                    _settings.MaximiseCoOccurrenceHighResidues,
                    _settings.IsSingleSamplePerDay
                );
            } else {
                return new ChronicDietaryExposureCalculator(
                    activeSubstances,
                    tdsReductionFactors,
                    consumptionsByFoodsAsMeasured,
                    concentrationModels,
                    individualDayIntakePruner,
                    processingFactorModels,
                    residueGenerator,
                    _settings.TotalDietStudy,
                    _settings.ReductionToLimitScenario
                );
            }
        }
    }
}
