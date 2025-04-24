using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    public class IntakeCalculatorFactory {
        private readonly bool _isSampleBased;
        private readonly bool _maximiseCoOccurrenceHighResidues;
        private readonly bool _isSingleSamplePerDay;
        private readonly int _numberOfMonteCarloIterations;
        private readonly ExposureType _exposureType;
        private readonly bool _totalDietStudy;
        private readonly bool _reductionToLimitScenario;

        public IntakeCalculatorFactory(
            bool isSampleBased,
            bool maximiseCoOccurrenceHighResidues,
            bool isSingleSamplePerDay,
            int numberOfMonteCarloIterations,
            ExposureType exposureType,
            bool totalDietStudy,
            bool reductionToLimitScenario
        ) {
            _isSampleBased = isSampleBased;
            _maximiseCoOccurrenceHighResidues = !isSampleBased && maximiseCoOccurrenceHighResidues;
            _isSingleSamplePerDay= isSingleSamplePerDay;
            _numberOfMonteCarloIterations = numberOfMonteCarloIterations;
            _exposureType = exposureType;
            _totalDietStudy = totalDietStudy;
            _reductionToLimitScenario = reductionToLimitScenario;
        }

        public DietaryExposureCalculatorBase Create(
            IProcessingFactorProvider processingFactorProvider,
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


            if (_exposureType == ExposureType.Acute) {
                return new AcuteDietaryExposureCalculator(
                    activeSubstances,
                    consumptionsByFoodsAsMeasured,
                    processingFactorProvider,
                    individualDayIntakePruner,
                    residueGenerator,
                    unitVariabilityCalculator,
                    consumptionsPerFoodAsMeasured,
                    _numberOfMonteCarloIterations,
                    _isSampleBased,
                    _maximiseCoOccurrenceHighResidues,
                    _isSingleSamplePerDay
                );
            } else {
                return new ChronicDietaryExposureCalculator(
                    activeSubstances,
                    tdsReductionFactors,
                    consumptionsByFoodsAsMeasured,
                    concentrationModels,
                    individualDayIntakePruner,
                    processingFactorProvider,
                    residueGenerator,
                    _totalDietStudy,
                    _reductionToLimitScenario
                );
            }
        }
    }
}
