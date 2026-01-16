using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.FoodConcentrationModelBuilders {

    /// <summary>
    /// Builder class for constructing concentration models.
    /// </summary>
    public sealed class FoodConcentrationModelsBuilder(
        IConcentrationModelCalculationSettings settings
    ) {
        private readonly IConcentrationModelCalculationSettings _settings = settings;

        /// <summary>
        /// Creates the concentration models for the specified food substance tuples.
        /// </summary>
        public IDictionary<(Food, Compound), ConcentrationModel> Create(
            ICollection<(Food Food, Compound Substance)> foodCompounds,
            IDictionary<(Food, Compound), FoodSubstanceResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> substanceAuthorisations,
            ConcentrationUnit concentrationUnit,
            CompositeProgressState progressState = null
        ) {
            var modelFactory = new FoodConcentrationModelFactory(_settings);
            var result = foodCompounds
                .Select(r => {
                    var key = (r.Food, r.Substance);
                    var compoundResidueCollection = compoundResidueCollections.TryGetValue(key, out var value)
                        ? value
                        : null;
                    var concentrationDistribution = concentrationDistributions?.TryGetValue(key, out var distribution) ?? false
                        ? distribution
                        : null;
                    var maximumResidueLimit = maximumConcentrationLimits?.TryGetValue(key, out var concentrationLimit) ?? false
                        ? concentrationLimit
                        : null;
                    var occurrenceFrequency = occurrenceFractions?.TryGetValue(key, out var fraction) ?? false
                        ? fraction.OccurrenceFrequency
                        : double.NaN;
                    if (_settings.RestrictLorImputationToAuthorisedUses) {
                        // Substances is considered authorised if an authorisation record exists for either for the
                        // food/substance combination itself, or the food has a base-food (e.g. because it is a processed
                        // commodity) and an authorisation record exists for the combination of base-food and substance.
                        var authorised = substanceAuthorisations.ContainsKey(key)
                            || key.Food.BaseFood != null && substanceAuthorisations.ContainsKey((key.Food.BaseFood, key.Substance));
                        if (!authorised) {
                            // Set the occurrence frequency to be equal to the detected number of positives.
                            occurrenceFrequency = compoundResidueCollections.TryGetValue(key, out var collection)
                                ? collection.FractionPositives : 0D;
                        }
                    }
                    return (
                        Key: key,
                        Model: createModelAndCalculateParameters(
                            modelFactory,
                            key.Food,
                            key.Substance,
                            compoundResidueCollection,
                            concentrationDistribution,
                            maximumResidueLimit,
                            occurrenceFrequency,
                            concentrationUnit
                       )
                    );
                })
                .ToDictionary(r => r.Key, r => r.Model);
            return result;
        }

        /// <summary>
        /// Creates the concentration models for all combinations of the specified
        /// foods and substances.
        /// </summary>
        public IDictionary<(Food, Compound), ConcentrationModel> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<(Food, Compound), FoodSubstanceResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> substanceAuthorisations,
            ConcentrationUnit concentrationUnit,
            CompositeProgressState progressState = null
        ) {
            var foodSubstances = foods
                .SelectMany(r => substances, (f, s) => (Food: f, Substance: s))
                .ToList();
            var result = Create(
                foodSubstances,
                compoundResidueCollections,
                concentrationDistributions,
                maximumConcentrationLimits,
                occurrenceFractions,
                substanceAuthorisations,
                concentrationUnit,
                progressState
            );
            return result;
        }

        /// <summary>
        /// Creates the concentration models for an uncertainty run based on the nominal models.
        /// </summary>
        public IDictionary<(Food, Compound), ConcentrationModel> CreateUncertain(
            IDictionary<(Food Food, Compound Substance), ConcentrationModel> concentrationModels,
            IDictionary<(Food, Compound), FoodSubstanceResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            bool reSampleConcentrations,
            bool isParametric,
            ConcentrationUnit concentrationUnit,
            int? randomSeed
        ) {
            var modelFactory = new FoodConcentrationModelFactory(_settings);
            var result = new Dictionary<(Food, Compound), ConcentrationModel>();
            var models = concentrationModels
                .Select(model => {
                    var key = model.Key;
                    var compoundResidueCollection = compoundResidueCollections.TryGetValue(key, out var value)
                        ? value
                        : null;
                    var concentrationDistribution = concentrationDistributions?.TryGetValue(key, out var distribution) ?? false
                        ? distribution
                        : null;
                    var maximumResidueLimit = maximumConcentrationLimits?.TryGetValue(key, out var concentrationLimit) ?? false
                        ? concentrationLimit
                        : null;
                    var occurrenceFrequency = occurrenceFractions?.TryGetValue(key, out var fraction) ?? false
                        ? fraction.OccurrenceFrequency
                        : double.NaN;

                    if (_settings.RestrictLorImputationToAuthorisedUses) {
                        var authorised = (substanceAuthorisations?.TryGetValue(key, out var authorisation) ?? false)
                            || key.Food.BaseFood != null
                            && (substanceAuthorisations?.TryGetValue((key.Food.BaseFood, key.Substance), out var substAuthorisation) ?? false);
                        occurrenceFrequency = authorised
                            ? occurrenceFrequency
                            : compoundResidueCollections.TryGetValue(key, out var collection)
                                ? collection.FractionPositives : 0D;
                    }
                    if (reSampleConcentrations) {
                        // Model already exists, but we want to re-create it
                        if (isParametric) {
                            if (model.Value.IsParametric) {
                                var seed = RandomUtils.CreateSeed(randomSeed.Value, key.Food.Code, key.Substance.Code);
                                var random = new McraRandomGenerator(seed);
                                // If the model is suitable for parametric uncertainty, then use it, otherwise asume bootstrap
                                model.Value.DrawParametricUncertainty(random);
                            } else {
                                // Bootstrap using the "old" concentration model type (i.e., the model fitted in the nominal run)
                                var newModel = modelFactory.CreateModelAndCalculateParameters(
                                    key.Food,
                                    key.Substance,
                                    model.Value.ModelType,
                                    compoundResidueCollection,
                                    concentrationDistribution,
                                    maximumResidueLimit,
                                    occurrenceFrequency,
                                    concentrationUnit);
                                model = new KeyValuePair<(Food, Compound), ConcentrationModel>(key, newModel);
                            }
                        } else {
                            // Bootstrap using the original concentration model (i.e., the model fitted in the nominal run)
                            var newModel = modelFactory.CreateModelAndCalculateParameters(
                                key.Food,
                                key.Substance,
                                model.Value.ModelType,
                                compoundResidueCollection,
                                concentrationDistribution,
                                maximumResidueLimit,
                                occurrenceFrequency,
                                concentrationUnit
                            );
                            model = new KeyValuePair<(Food, Compound), ConcentrationModel>(key, newModel);
                        }
                    }
                    return model;
                })
                .ToList();
            models.ForEach(r => result.Add(r.Key, r.Value));
            return result;
        }

        /// <summary>
        /// Generates the concentration model for the specified food and substance.
        /// </summary>
        private ConcentrationModel createModelAndCalculateParameters(
            FoodConcentrationModelFactory modelFactory,
            Food food,
            Compound substance,
            FoodSubstanceResidueCollection compoundResidueCollection,
            ConcentrationDistribution concentrationDistribution,
            ConcentrationLimit maximumConcentrationLimits,
            double occurrenceFrequency,
            ConcentrationUnit concentrationUnit
        ) {
            ConcentrationModelType modelType;
            modelType = _settings.ConcentrationModelTypesPerFoodCompound
                .FirstOrDefault(c => c.FoodCode == food.Code && c.SubstanceCode == substance.Code)?
                .ModelType ?? _settings.DefaultConcentrationModel;
            var model = modelFactory.CreateModelAndCalculateParameters(
                food,
                substance,
                modelType,
                compoundResidueCollection,
                concentrationDistribution,
                maximumConcentrationLimits,
                occurrenceFrequency,
                concentrationUnit
            );
            return model;
        }
    }
}
