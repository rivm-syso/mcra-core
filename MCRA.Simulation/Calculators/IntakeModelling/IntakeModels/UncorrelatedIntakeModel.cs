using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Base class for uncorrelated exposure models (BBN, LNN0)
    /// </summary>
    /// <typeparam name="TFrequencyModel"></typeparam>
    /// <typeparam name="TAmountsModel"></typeparam>
    public abstract class UncorrelatedIntakeModel<TFrequencyModel, TAmountsModel>
        : IntakeModel, IUncorrelatedIntakeModel
        where TFrequencyModel : FrequencyModel, new()
        where TAmountsModel : AmountsModelBase, new()
    {

        public TransformType TransformType { get; set; }

        public IIntakeModelCalculationSettings AmountModelSettings { get; set; }

        public IIntakeModelCalculationSettings FrequencyModelSettings { get; set; }

        public int NumberOfMonteCarloIterations { get; set; }

        public double FixedDispersion { get; set; }
        public double VarianceRatio { get; set; }

        public List<double> PredictionLevels { get; set; }

        public FrequencyModelSummary FrequencyModelSummary { get; set; }
        public AmountsModelSummary AmountsModelSummary { get; set; }

        public bool IsAcuteCovariateModelling { get; set; }

        public TFrequencyModel FrequencyModel { get; set; }
        public TAmountsModel AmountModel { get; protected set; }

        private MonteCarloIntegrator<TFrequencyModel, TAmountsModel> _monteCarloIntegrator;

        /// <summary>
        /// Creates a new <see cref="UncorrelatedIntakeModel{TFrequencyModel, TAmountsModel}"/> instance.
        /// </summary>
        /// <param name="frequencyModelSettings"></param>
        /// <param name="amountModelSettings"></param>
        /// <param name="predictionLevels"></param>
        public UncorrelatedIntakeModel(
            IIntakeModelCalculationSettings frequencyModelSettings,
            IIntakeModelCalculationSettings amountModelSettings,
            List<double> predictionLevels = null
        ) {
            FrequencyModelSettings = frequencyModelSettings;
            AmountModelSettings = amountModelSettings;
            PredictionLevels = predictionLevels;
        }

        public virtual void Initialize() {
            FrequencyModel = new TFrequencyModel {
                FixedDispersion = FixedDispersion,
                MinDegreesOfFreedom = FrequencyModelSettings.MinDegreesOfFreedom,
                MaxDegreesOfFreedom = FrequencyModelSettings.MaxDegreesOfFreedom,
                CovariateModel = FrequencyModelSettings.CovariateModelType,
                Function = FrequencyModelSettings.FunctionType,
                TestingLevel = FrequencyModelSettings.TestingLevel,
                TestingMethod = FrequencyModelSettings.TestingMethod
            };

            AmountModel = new TAmountsModel {
                MinDegreesOfFreedom = AmountModelSettings.MinDegreesOfFreedom,
                MaxDegreesOfFreedom = AmountModelSettings.MaxDegreesOfFreedom,
                CovariateModel = AmountModelSettings.CovariateModelType,
                Function = AmountModelSettings.FunctionType,
                TestingLevel = AmountModelSettings.TestingLevel,
                TestingMethod = AmountModelSettings.TestingMethod,
                TransformType = TransformType,
                IsAcuteCovariateModelling = IsAcuteCovariateModelling,
                VarianceRatio = VarianceRatio,
                NumberOfMonteCarloIterations = NumberOfMonteCarloIterations
            };

            _monteCarloIntegrator = new MonteCarloIntegrator<TFrequencyModel, TAmountsModel>() {
                FrequencyModel = FrequencyModel,
                AmountsModel = AmountModel,
                NumberOfMonteCarloIterations = NumberOfMonteCarloIterations
            };
        }

        public override void CalculateParameters(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes
        ) {
            Initialize();

            var covariateGroupCalculator = new CovariateGroupCalculator(
                PredictionLevels,
                FrequencyModelSettings.CovariateModelType,
                AmountModelSettings.CovariateModelType
            );

            DataBasedCovariateGroups = covariateGroupCalculator
                .ComputeDataBasedCovariateGroups(individualDayIntakes);

            SpecifiedPredictionCovariateGroups = covariateGroupCalculator
                .ComputeSpecifiedPredictionsCovariateGroups(individualDayIntakes);

            var individualIntakeFrequencies = IndividualFrequencyCalculator
                .Compute(individualDayIntakes);
            FrequencyModelSummary = FrequencyModel
                .CalculateParameters(individualIntakeFrequencies, PredictionLevels);

            var individualIntakeAmounts = SimpleIndividualDayIntakesCalculator
                .ComputeIndividualAmounts(individualDayIntakes);
            AmountsModelSummary = AmountModel.CalculateParameters(individualIntakeAmounts, PredictionLevels);
        }

        public override List<ConditionalUsualIntake> GetConditionalIntakes(int seed, CompositeProgressState progressState = null) {
            return _monteCarloIntegrator.CalculateConditionalIntakes(SpecifiedPredictionCovariateGroups, seed, progressState);
        }

        public override List<ModelBasedIntakeResult> GetMarginalIntakes(int seed, CompositeProgressState progressState = null) {
            return _monteCarloIntegrator.CalculateMarginalIntakes(DataBasedCovariateGroups, seed, progressState);
        }

        public override List<ModelAssistedIntake> GetIndividualIntakes(int seed) {
            return _monteCarloIntegrator.CalculateIndividualIntakes(seed);
        }
    }
}
