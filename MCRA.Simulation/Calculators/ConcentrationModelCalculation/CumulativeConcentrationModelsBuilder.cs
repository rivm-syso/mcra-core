using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation {

    /// <summary>
    /// Builder class for constructing concentration models.
    /// </summary>
    public sealed class CumulativeConcentrationModelsBuilder {
        private readonly IConcentrationModelCalculationSettings _settings;

        public CumulativeConcentrationModelsBuilder(IConcentrationModelCalculationSettings settings) { 
            _settings = settings; 
        }

        public Dictionary<Food, ConcentrationModel> Create(
            ICollection<Food> foodsAsMeasured,
            IDictionary<Food, CompoundResidueCollection> cumulativeCompoundResidueCollections,
            Compound cumulativeCompound,
            ConcentrationUnit concentrationUnit
        ) {
            var result = new Dictionary<Food, ConcentrationModel>();
            var modelFactory = new ConcentrationModelFactory(_settings);
            foreach (var food in foodsAsMeasured) {
                var compoundResidueCollection = cumulativeCompoundResidueCollections.ContainsKey(food) ?
                    cumulativeCompoundResidueCollections[food] : null;
                var model = CreateCumulativeModelAndCalculateParameters(modelFactory, food, cumulativeCompound, compoundResidueCollection, concentrationUnit);
                result.Add(food, model);
            }
            return result;
        }

        public Dictionary<Food, ConcentrationModel> CreateUncertain(
            ICollection<Food> foodsAsMeasured,
            IDictionary<Food, CompoundResidueCollection> cumulativeCompoundResidueCollections,
            Compound cumulativeCompound,
            bool reSampleConcentrations,
            bool isParametric,
            ConcentrationUnit concentrationUnit,
            int? seed
        ) {
            if (reSampleConcentrations) {
                if (isParametric) {
                    var result = new Dictionary<Food, ConcentrationModel>();
                    var modelFactory = new ConcentrationModelFactory(_settings);
                    foreach (var food in foodsAsMeasured) {
                        var random = new McraRandomGenerator(RandomUtils.CreateSeed(seed.Value, food.Code));
                        var compoundResidueCollection = cumulativeCompoundResidueCollections.ContainsKey(food) ? cumulativeCompoundResidueCollections[food] : null;
                        var model = CreateCumulativeModelAndCalculateParameters(modelFactory, food, cumulativeCompound, compoundResidueCollection, concentrationUnit);
                        if (model.IsParametric()) {
                            model.DrawParametricUncertainty(random);
                        } else {
                            // Bootstrap using the "old" concentration model type (i.e., the model fitted in the nominal run)
                            if (cumulativeCompoundResidueCollections.TryGetValue(food, out CompoundResidueCollection item)) {
                                cumulativeCompoundResidueCollections[food] = CompoundResidueCollectionsBuilder.Resample(item, random);
                            }
                            model = CreateCumulativeModelAndCalculateParameters(modelFactory, food, cumulativeCompound, compoundResidueCollection, concentrationUnit);
                        }
                        result.Add(food, model);
                    }
                    return result;
                } else {
                    // Create new cumulative concentration models based on bootstrapped data.
                    return Create(foodsAsMeasured, cumulativeCompoundResidueCollections, cumulativeCompound, concentrationUnit);
                }
            }
            return null;
        }

        /// <summary>
        /// Generates the concentration model for the specified food and substance.
        /// </summary>
        /// <param name="modelFactory"></param>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="cumulativeCompoundResidueCollection"></param>
        /// <returns></returns>
        public ConcentrationModel CreateCumulativeModelAndCalculateParameters(
            ConcentrationModelFactory modelFactory,
            Food food,
            Compound substance,
            CompoundResidueCollection cumulativeCompoundResidueCollection,
            ConcentrationUnit concentrationUnit
        ) {
            var desiredModelType = _settings.ConcentrationModelTypesPerFoodCompound
               .FirstOrDefault(c => c.CodeFood == food.Code)?.ConcentrationModelType ?? _settings.DefaultConcentrationModel;
            var cumulativeModelType = getCumulativeConcentrationModelType(desiredModelType);
            var occurrenceFrequency = 1D - cumulativeCompoundResidueCollection.FractionZeros;
            var model = modelFactory.CreateModelAndCalculateParameters(food, substance, cumulativeModelType, cumulativeCompoundResidueCollection, null, null, occurrenceFrequency, concentrationUnit);
            return model;
        }

        /// <summary>
        /// Returns the pessimistic fallback concentration model type that should be used if the given concentration model type does not fit.
        /// </summary>
        /// <param name="defaultConcentrationModelType"></param>
        /// <returns></returns>
        private static ConcentrationModelType getCumulativeConcentrationModelType(ConcentrationModelType defaultConcentrationModelType) {
            switch (defaultConcentrationModelType) {
                case ConcentrationModelType.ZeroSpikeCensoredLogNormal:
                case ConcentrationModelType.CensoredLogNormal:
                case ConcentrationModelType.NonDetectSpikeTruncatedLogNormal:
                case ConcentrationModelType.NonDetectSpikeLogNormal:
                    return ConcentrationModelType.NonDetectSpikeLogNormal;
                case ConcentrationModelType.MaximumResidueLimit:
                case ConcentrationModelType.Empirical:
                    return ConcentrationModelType.Empirical;
                default:
                    return ConcentrationModelType.Empirical;
            }
        }
    }
}
