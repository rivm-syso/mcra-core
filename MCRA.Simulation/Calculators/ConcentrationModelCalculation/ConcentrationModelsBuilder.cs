﻿using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation {

    /// <summary>
    /// Builder class for constructing concentration models.
    /// </summary>
    public sealed class ConcentrationModelsBuilder {
        private IConcentrationModelCalculationSettings _settings;

        public ConcentrationModelsBuilder(
            IConcentrationModelCalculationSettings settings
         ) {
            _settings = settings;
        }

        /// <summary>
        /// Creates the concentration models for the specified food substance tuples.
        /// </summary>
        /// <param name="foodCompounds"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="concentrationDistributions"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="occurrenceFractions"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), ConcentrationModel> Create(
            ICollection<(Food Food, Compound Substance)> foodCompounds,
            IDictionary<(Food, Compound), CompoundResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), double> occurrenceFractions,
            ConcentrationUnit concentrationUnit,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new CancellationToken();
            var modelFactory = new ConcentrationModelFactory(_settings);
            var models = foodCompounds
                .Select(r => {
                    var key = (r.Food, r.Substance);
                    var compoundResidueCollection = (compoundResidueCollections?.ContainsKey(key) ?? false) ? compoundResidueCollections[key] : null;
                    var concentrationDistribution = (concentrationDistributions?.ContainsKey(key) ?? false) ? concentrationDistributions[key] : null;
                    var maximumResidueLimit = (maximumConcentrationLimits?.ContainsKey(key) ?? false) ? maximumConcentrationLimits[key] : null;
                    var occurrenceFrequency = (occurrenceFractions?.ContainsKey(key) ?? false) ? occurrenceFractions[key] : double.NaN;
                    return (
                        Key: key,
                        Model: createModelAndCalculateParameters(modelFactory, key.Food, key.Substance, compoundResidueCollection, concentrationDistribution, maximumResidueLimit, occurrenceFrequency, concentrationUnit)
                    );
                })
                .ToList();
            var result = new Dictionary<(Food, Compound), ConcentrationModel>();
            foreach (var model in models) {
                result.Add((model.Key.Food, model.Key.Substance), model.Model);
            }
            return result;
        }

        /// <summary>
        /// Creates the concentration models for all combinations of the specified
        /// foods and substances.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="concentrationDistributions"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="occurrenceFractions"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), ConcentrationModel> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<(Food, Compound), CompoundResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), double> occurrenceFractions,
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
                concentrationUnit,
                progressState
            );
            return result;
        }

        /// <summary>
        /// Creates the concentration models for an uncertainty run based on the nominal models.
        /// </summary>
        /// <param name="concentrationModels"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="concentrationDistributions"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="occurrenceFractions"></param>
        /// <param name="reSampleConcentrations"></param>
        /// <param name="isParametric"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), ConcentrationModel> CreateUncertain(
            IDictionary<(Food Food, Compound Substance), ConcentrationModel> concentrationModels,
            IDictionary<(Food, Compound), CompoundResidueCollection> compoundResidueCollections,
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            bool reSampleConcentrations,
            bool isParametric,
            ConcentrationUnit concentrationUnit,
            int? randomSeed
        ) {
            var modelFactory = new ConcentrationModelFactory(_settings);
            var result = new Dictionary<(Food, Compound), ConcentrationModel>();
            var models = concentrationModels
                .Select(record => {
                    var model = record;
                    var key = model.Key;
                    var compoundResidueCollection = (compoundResidueCollections?.ContainsKey(key) ?? false) ? compoundResidueCollections[key] : null;
                    var concentrationDistribution = (concentrationDistributions?.ContainsKey(key) ?? false) ? concentrationDistributions[key] : null;
                    var maximumResidueLimit = (maximumConcentrationLimits?.ContainsKey(key) ?? false) ? maximumConcentrationLimits[key] : null;
                    var occurrenceFrequency = (occurrenceFractions?.ContainsKey(key) ?? false) ? occurrenceFractions[key].OccurrenceFrequency : double.NaN;
                    if (reSampleConcentrations) {
                        // Model already exists, but we want to re-create it
                        if (isParametric) {
                            if (model.Value.IsParametric()) {
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
                        } else if (!isParametric) {
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
        /// <param name="modelFactory"></param>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="compoundResidueCollection"></param>
        /// <param name="concentrationDistribution"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="occurrenceFrequency"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        private ConcentrationModel createModelAndCalculateParameters(
            ConcentrationModelFactory modelFactory,
            Food food,
            Compound substance,
            CompoundResidueCollection compoundResidueCollection,
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
