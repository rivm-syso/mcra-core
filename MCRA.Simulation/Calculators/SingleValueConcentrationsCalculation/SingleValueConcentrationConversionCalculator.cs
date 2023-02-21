using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation {
    public class SingleValueConcentrationConversionCalculator {

        public IDictionary<(Food, Compound), SingleValueConcentrationModel> Compute(
            ICollection<Compound> activeSubstances,
            IDictionary<(Food, Compound), SingleValueConcentrationModel> concentrationSingleValues,
            ICollection<DeterministicSubstanceConversionFactor> deterministicSubstanceConversionFactor
        ) {
            var result = new Dictionary<(Food, Compound), SingleValueConcentrationModel>();
            var conversionDictionary = new Dictionary<(Compound, Food), DeterministicSubstanceConversionFactor>();
            foreach (var item in deterministicSubstanceConversionFactor) {
                conversionDictionary[(item.MeasuredSubstance, item.Food)] = item;
            }
            foreach (var concentration in concentrationSingleValues.Values) {
                var measuredSubstance = concentration.Substance;
                var food = concentration.Food;
                if (conversionDictionary.TryGetValue((measuredSubstance, food), out var conversion)) {
                    var activeSubstanceRecord = createActiveSubstanceSingleValueConcentrationModel(concentration, conversion);
                    addActiveSubstanceConcentration(result, concentration, activeSubstanceRecord);
                } else if (conversionDictionary.TryGetValue((measuredSubstance, null), out conversion)) {
                    var activeSubstanceRecord = createActiveSubstanceSingleValueConcentrationModel(concentration, conversion);
                    addActiveSubstanceConcentration(result, concentration, activeSubstanceRecord);
                } else if (activeSubstances.Contains(measuredSubstance)) {
                    var activeSubstanceRecord = concentration.Clone();
                    addActiveSubstanceConcentration(result, concentration, activeSubstanceRecord);
                }
            }
            return result;
        }

        private static void addActiveSubstanceConcentration(
            IDictionary<(Food, Compound), SingleValueConcentrationModel> result,
            SingleValueConcentrationModel concentration,
            SingleValueConcentrationModel activeSubstanceRecord
        ) {
            if (result.ContainsKey((concentration.Food, activeSubstanceRecord.Substance))) {
                throw new Exception($"Encountered multiple measured substance concentrations leading to the same active substance conversions for food {concentration.Food.Name} ({concentration.Food.Code}) and active substance {concentration.Substance.Name} ({concentration.Substance.Code})");
            }
            result.Add((concentration.Food, activeSubstanceRecord.Substance), activeSubstanceRecord);
        }

        private static SingleValueConcentrationModel createActiveSubstanceSingleValueConcentrationModel(
            SingleValueConcentrationModel concentration,
            DeterministicSubstanceConversionFactor conversionRecord
        ) {
            var conversionFactor = conversionRecord.ConversionFactor;
            return new ConvertedSingleValueConcentrationModel() {
                Food = concentration.Food,
                Substance = conversionRecord.ActiveSubstance,
                HighestConcentration = conversionFactor * concentration.HighestConcentration,
                MeanConcentration = conversionFactor * concentration.MeanConcentration,
                Loq = conversionFactor * concentration.Loq,
                Mrl = conversionFactor * concentration.Mrl,
                Percentiles = concentration.Percentiles?.Select(r => (r.Percentage, conversionFactor * r.Percentile)).ToList(),
                MeasuredSingleValueConcentrationModel = concentration,
                ConversionFactor = conversionFactor
            };
        }
    }
}
