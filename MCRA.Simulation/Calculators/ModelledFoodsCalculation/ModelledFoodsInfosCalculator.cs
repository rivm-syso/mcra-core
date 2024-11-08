using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.ModelledFoodsCalculation {
    public sealed class ModelledFoodsInfosCalculator {
        private readonly IModelledFoodsInfosCalculatorSettings _settings;

        public ModelledFoodsInfosCalculator(IModelledFoodsInfosCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Returns all samples that apply to a food/substance combination.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="singleValueConcentrations"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <returns></returns>
        public ICollection<ModelledFoodInfo> Compute(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), SingleValueConcentrationModel> singleValueConcentrations,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits
        ) {
            singleValueConcentrations = _settings.DeriveModelledFoodsFromSingleValueConcentrations
                ? singleValueConcentrations : null;
            maximumConcentrationLimits = _settings.UseWorstCaseValues
                ? maximumConcentrationLimits : null;

            var modelledFoodsInfoRecords = new Dictionary<(Food, Compound), ModelledFoodInfo>();

            // Check sample-based concentrations
            if (sampleCompoundCollections != null) {
                foreach (var food in foods) {
                    if (sampleCompoundCollections.TryGetValue(food, out var collection)) {
                        //create a dictionary of substances with flag indicating any positive measerements
                        var residuesWithPositives = collection.SampleCompoundRecords
                            .SelectMany(s => s.SampleCompounds)
                            .Where(v => !v.Value.IsMissingValue)
                            .GroupBy(s => s.Key, v => v.Value.IsPositiveResidue)
                            .ToDictionary(s => s.Key, v => v.Any(t => t));

                        foreach (var substance in substances) {
                            if (residuesWithPositives.TryGetValue(substance, out var hasPositives)) {
                                var record = getOrAdd(food, substance, modelledFoodsInfoRecords);
                                record.HasMeasurements = true;
                                record.HasPositiveMeasurements = hasPositives;
                            }
                        }
                    }
                }
            }

            // Check for single value concentrations
            if (singleValueConcentrations != null) {
                foreach (var food in foods) {
                    foreach (var substance in substances) {
                        if (singleValueConcentrations.TryGetValue((food, substance), out var singleValueConcentration)) {
                            var hasPositiveMeasurements = singleValueConcentration.HasPositiveMeasurement();
                            var record = getOrAdd(food, substance, modelledFoodsInfoRecords);
                            record.HasMeasurements = singleValueConcentration.HasMeasurement();
                            record.HasPositiveMeasurements = hasPositiveMeasurements;
                        }
                    }
                }
            }

            // Check for MRLs
            if (maximumConcentrationLimits != null) {
                foreach (var food in foods) {
                    foreach (var substance in substances) {
                        if (maximumConcentrationLimits.TryGetValue((food, substance), out var mrl)) {
                            var record = getOrAdd(food, substance, modelledFoodsInfoRecords);
                            record.HasMrl = true;
                        }
                    }
                }
            }

            var result = modelledFoodsInfoRecords.Values.ToList();

            // If specified: throw out the foods with only censored values
            if (!_settings.FoodIncludeNonDetects) {
                var groupedInfos = result.GroupBy(f => f.Food);
                var nonDetectsOnlyRecords = groupedInfos
                    .Where(gspfc => !gspfc.Any(spfc => spfc.HasMrl || spfc.HasPositiveMeasurements))
                    .Select(g => g.Key)
                    .ToHashSet();
                result.RemoveAll(r => nonDetectsOnlyRecords.Contains(r.Food));
            }

            // If specified: throw out the foods with only censored values
            if (!_settings.SubstanceIncludeNonDetects) {
                var groupedInfos = result.GroupBy(f => f.Substance);
                var nonDetectsOnlyRecords = groupedInfos
                    .Where(gspfc => !gspfc.Any(spfc => spfc.HasMrl || spfc.HasPositiveMeasurements))
                    .Select(g => g.Key)
                    .ToHashSet();
                result.RemoveAll(r => nonDetectsOnlyRecords.Contains(r.Substance));
            }

            return result;
        }

        private static ModelledFoodInfo getOrAdd(Food food, Compound substance, IDictionary<(Food, Compound), ModelledFoodInfo> modelledFoodsInfos) {
            if (!modelledFoodsInfos.TryGetValue((food, substance), out var infoRecord)) {
                infoRecord = new ModelledFoodInfo() {
                    Food = food,
                    Substance = substance
                };
                modelledFoodsInfos.Add((food, substance), infoRecord);
            }
            return infoRecord;
        }
    }
}
