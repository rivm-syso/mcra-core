using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation.ConsumptionUnitWeightGeneration;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Base class for all IntakeCalculators: algorithms for calculating the substance exposure for a given set of individuals.
    /// </summary>
    public abstract class DietaryExposureCalculatorBase {

        public bool _isSubstanceDependent;

        public IDictionary<(Individual idv, string day), List<ConsumptionsByModelledFood>> _consumptionsByFoodsAsMeasured;

        public DietaryExposureCalculatorBase(
            IDictionary<(Individual, string), List<ConsumptionsByModelledFood>> consumptionsByFoodsAsMeasured,
            IProcessingFactorProvider processingFactorProvider,
            ICollection<Compound> activeSubstances,
            IIndividualDayIntakePruner individualDayIntakePruner
        ) {
            if (activeSubstances != null) {
                _selectedSubstances = activeSubstances.OrderBy(r => r.Code, StringComparer.OrdinalIgnoreCase).ToHashSet();
            }
            _consumptionsByFoodsAsMeasured = consumptionsByFoodsAsMeasured;
            _processingFactorProvider = processingFactorProvider;
            _individualDayIntakePruner = individualDayIntakePruner;
            _isSubstanceDependent = _consumptionsByFoodsAsMeasured
                .Any(cfam => cfam.Value.Any(cr => cr.ConversionResultsPerCompound.Keys.Any(c => !string.IsNullOrEmpty(c.Code))));
        }

        public bool ModelConsumptionAmountUncertainty { get; set; }

        public ConsumptionUnitWeightGenerator UnitWeightGenerator { get; set; }

        protected ICollection<Compound> _selectedSubstances { get; set; }

        protected IIndividualDayIntakePruner _individualDayIntakePruner { get; set; }

        protected IProcessingFactorProvider _processingFactorProvider { get; set; }

        public abstract List<DietaryIndividualDayIntake> CalculateDietaryIntakes(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            ProgressState progressState,
            int randomSeed
        );

        public abstract Dictionary<Compound, List<ExposureRecord>> ComputeExposurePerCompoundRecords(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        );
    }
}
