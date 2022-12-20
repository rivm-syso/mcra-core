using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using System.Collections.Generic;
using System.Linq;
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
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            //deze ook het object meegeven?
            var modelFactory = new ConcentrationModelFactory(_settings);
            var models = foodCompounds
                //Not allowed for R DotNet Engine
                //.AsParallel()
                //.WithCancellation(cancelToken)
                //.WithDegreeOfParallelism(100)
                .Select(r => {
                    var key = (r.Food, r.Substance);
                    var compoundResidueCollection = (compoundResidueCollections?.ContainsKey(key) ?? false) ? compoundResidueCollections[key] : null;
                    var concentrationDistribution = (concentrationDistributions?.ContainsKey(key) ?? false) ? concentrationDistributions[key] : null;
                    var maximumResidueLimit = (maximumConcentrationLimits?.ContainsKey(key) ?? false) ? maximumConcentrationLimits[key] : null;
                    var occurrenceFrequency = (occurrenceFractions?.ContainsKey(key) ?? false) ? occurrenceFractions[key] : double.NaN;
                    return createModelAndCalculateParameters(modelFactory, key.Food, key.Substance, compoundResidueCollection, concentrationDistribution, maximumResidueLimit, occurrenceFrequency, concentrationUnit);
                })
                .ToList();
            var result = new Dictionary<(Food, Compound), ConcentrationModel>();
            foreach (var model in models) {
                result.Add((model.Food, model.Compound), model);
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
            ICollection<ConcentrationModel> concentrationModels,
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
                    var key = (model.Food, model.Compound);
                    var compoundResidueCollection = (compoundResidueCollections?.ContainsKey(key) ?? false) ? compoundResidueCollections[key] : null;
                    var concentrationDistribution = (concentrationDistributions?.ContainsKey(key) ?? false) ? concentrationDistributions[key] : null;
                    var maximumResidueLimit = (maximumConcentrationLimits?.ContainsKey(key) ?? false) ? maximumConcentrationLimits[key] : null;
                    var occurrenceFrequency = (occurrenceFractions?.ContainsKey(key) ?? false) ? occurrenceFractions[key].OccurrenceFrequency : double.NaN;
                    if (reSampleConcentrations) {
                        // Model already exists, but we want to re-create it
                        if (isParametric) {
                            if (model.IsParametric()) {
                                var seed = RandomUtils.CreateSeed(randomSeed.Value, key.Food.Code, key.Compound.Code);
                                var random = new McraRandomGenerator(seed, true);
                                // If the model is suitable for parametric uncertainty, then use it, otherwise asume bootstrap
                                model.DrawParametricUncertainty(random);
                            } else {
                                // Bootstrap using the "old" concentration model type (i.e., the model fitted in the nominal run)
                                model = modelFactory.CreateModelAndCalculateParameters(
                                    key.Food,
                                    key.Compound,
                                    model.ModelType,
                                    compoundResidueCollection,
                                    concentrationDistribution,
                                    maximumResidueLimit,
                                    occurrenceFrequency,
                                    concentrationUnit);
                            }
                        } else if (!isParametric) {
                            // Bootstrap using the original concentration model (i.e., the model fitted in the nominal run)
                            model = modelFactory.CreateModelAndCalculateParameters(
                                key.Food,
                                key.Compound,
                                model.ModelType,
                                compoundResidueCollection,
                                concentrationDistribution,
                                maximumResidueLimit,
                                occurrenceFrequency,
                                concentrationUnit
                            );
                        }
                    }
                    return model;
                })
                .ToList();
            models.ForEach(r => result.Add((r.Food, r.Compound), r));
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
                .FirstOrDefault(c => c.CodeFood == food.Code && c.CodeCompound == substance.Code)?
                .ConcentrationModelType ?? _settings.DefaultConcentrationModel;
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
