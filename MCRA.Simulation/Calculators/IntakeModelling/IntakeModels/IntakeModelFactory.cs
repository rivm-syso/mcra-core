using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class IntakeModelFactory {

        private IIntakeModelCalculationSettings _frequencyModelSettings;
        private IIntakeModelCalculationSettings _amountModelSettings;
        private IISUFModelCalculationSettings _isufModelSettings;

        private readonly int _numberOfMonteCarloIterations;
        private readonly double _intervals;
        private readonly double[] _userSpecifiedPredictionLevels;
        private readonly double? _dispersion;
        private readonly double? _varianceRatio;

        public IntakeModelFactory(
            IIntakeModelCalculationSettings frequencyModelSettings,
            IIntakeModelCalculationSettings amountModelSettings,
            IISUFModelCalculationSettings isufModelSettings,
            int numberOfMonteCarloIterations,
            double intervals,
            double[] extraPredictionLevels,
            double? dispersion,
            double? varianceRatio
        ) {
            _frequencyModelSettings = frequencyModelSettings;
            _amountModelSettings = amountModelSettings;
            _isufModelSettings = isufModelSettings;
            _numberOfMonteCarloIterations = numberOfMonteCarloIterations;
            _intervals = intervals;
            _userSpecifiedPredictionLevels = extraPredictionLevels;
            _dispersion = dispersion;
            _varianceRatio = varianceRatio;
        }

        public IntakeModel CreateIntakeModel(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            bool covariateModelling,
            ExposureType exposureType,
            IntakeModelType intakeModelType,
            TransformType transformType
        ) {
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(
                dietaryIndividualDayIntakes,
                _intervals,
                _userSpecifiedPredictionLevels
            );

            var isCovariateModelling = covariateModelling && exposureType == ExposureType.Acute;
            var intakeModel = createIntakeModel(
                intakeModelType,
                transformType,
                isCovariateModelling,
                predictionLevels
            );
            if (intakeModelType == IntakeModelType.LNN) {
                // fallback: for no incidental intake, no frequency model is fitted and LNN0 is
                // the falbackmodel, then InitalLNN0Model != null
                intakeModel = (intakeModel as LNNModel).Lnn0Model ?? intakeModel;
            }
            return intakeModel;
        }

        public CompositeIntakeModel CreateCompositeIntakeModel(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Food> allFoodsAsMeasured,
            ICollection<IntakeModelPerCategory> intakeModelsPerCategory
        ) {
            var partialIntakeModels = new List<ModelThenAddPartialIntakeModel>();
            var categories = new List<Food>();

            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(
                dietaryIndividualDayIntakes,
                _intervals,
                _userSpecifiedPredictionLevels
            );
            var counter = 0;
            foreach (var category in intakeModelsPerCategory) {
                var foodAsMeasuredCodes = category.FoodsAsMeasured.ToList();
                var foodsAsMeasured = allFoodsAsMeasured
                    .Where(f => foodAsMeasuredCodes
                    .Contains(f.Code))
                    .ToList();
                var intakeModelPerCategory = createIntakeModel(
                    category.ModelType,
                    category.TransformType,
                    false,
                    predictionLevels
                );
                partialIntakeModels.Add(new ModelThenAddPartialIntakeModel() {
                    ModelIndex = counter++,
                    FoodsAsMeasured = foodsAsMeasured,
                    IntakeModel = intakeModelPerCategory
                });
                categories.AddRange(foodsAsMeasured);
            }
            var allPositiveIntakeFoods = dietaryIndividualDayIntakes
                .SelectMany(r => r.IntakesPerFood
                    .Where(ipf => ipf.IsPositiveIntake())
                    .Select(ipf => ipf.FoodAsMeasured))
                .Distinct();

            var remainingCategories = allPositiveIntakeFoods
                .Where(r => !categories.Contains(r))
                .ToList();
            if (remainingCategories.Any()) {
                var intakeModelPerCategory = createIntakeModel(
                    IntakeModelType.OIM,
                    TransformType.NoTransform,
                    false,
                    predictionLevels
                );
                partialIntakeModels.Add(new ModelThenAddPartialIntakeModel() {
                    ModelIndex = counter++,
                    FoodsAsMeasured = remainingCategories,
                    IntakeModel = intakeModelPerCategory
                });
            }
            return new CompositeIntakeModel() {
                PartialModels = partialIntakeModels
            };
        }

        private IntakeModel createIntakeModel(
            IntakeModelType intakeModelType,
            TransformType transformType,
            bool isCovariateModelling,
            List<double> predictionLevels
        ) {
            switch (intakeModelType) {
                case IntakeModelType.OIM:
                    return new OIMModel();
                case IntakeModelType.BBN:
                    return new BBNModel(
                        _frequencyModelSettings,
                        _amountModelSettings,
                        predictionLevels
                    ) {
                        TransformType = transformType,
                        NumberOfMonteCarloIterations = _numberOfMonteCarloIterations,
                        IsAcuteCovariateModelling = isCovariateModelling,
                        FixedDispersion = _dispersion.Value,
                        VarianceRatio = _varianceRatio.Value
                    };
                case IntakeModelType.ISUF:
                    return new ISUFModel(
                        transformType,
                        _isufModelSettings
                    );
                case IntakeModelType.LNN:
                    return new LNNModel(
                        _frequencyModelSettings,
                        _amountModelSettings,
                        predictionLevels
                    ) {
                        TransformType = transformType,
                        NumberOfMonteCarloIterations = _numberOfMonteCarloIterations
                    };
                case IntakeModelType.LNN0:
                    return new LNN0Model(
                        _frequencyModelSettings,
                        _amountModelSettings,
                        predictionLevels
                    ) {
                        TransformType = transformType,
                        NumberOfMonteCarloIterations = _numberOfMonteCarloIterations,
                        IsAcuteCovariateModelling = isCovariateModelling,
                        FixedDispersion = _dispersion.Value,
                        VarianceRatio = _varianceRatio.Value
                    };
            }
            return null;
        }
    }
}
