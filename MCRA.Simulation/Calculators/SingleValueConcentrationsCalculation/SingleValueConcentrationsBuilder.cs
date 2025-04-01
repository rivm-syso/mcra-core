using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation {
    public sealed class SingleValueConcentrationsBuilder {

        /// <summary>
        /// Creates single value concentration models from the data.
        /// </summary>
        /// <param name="concentrationSingleValues"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), SingleValueConcentrationModel> Create(
            ICollection<ConcentrationSingleValue> concentrationSingleValues,
            ConcentrationUnit concentrationUnit
        ) {
            var result = new Dictionary<(Food, Compound), SingleValueConcentrationModel>();
            var valuesLookup = concentrationSingleValues.ToLookup(r => (r.Food, r.Substance));
            foreach (var group in valuesLookup) {
                var food = group.Key.Food;
                var substance = group.Key.Substance;
                var record = new SingleValueConcentrationModel() {
                    Food = food,
                    Substance = substance,
                    Loq = getValue(group, ConcentrationValueType.LOQ, concentrationUnit),
                    HighestConcentration = getValue(group, ConcentrationValueType.HighestConcentration, concentrationUnit),
                    Mrl = getValue(group, ConcentrationValueType.MRL, concentrationUnit),
                    Percentiles = getPercentiles(group, concentrationUnit)
                };
                result.Add((food, substance), record);
            }
            return result;
        }

        private double getValue(
            IEnumerable<ConcentrationSingleValue> values,
            ConcentrationValueType valueType,
            ConcentrationUnit concentrationUnit
        ) {
            if (values != null) {
                var record = values.FirstOrDefault(r => r.ValueType == valueType);
                if (record != null) {
                    var unitConversionFactor = record.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    return unitConversionFactor * record.Value;
                }
            }
            return double.NaN;
        }

        private List<(double, double)> getPercentiles(
            IEnumerable<ConcentrationSingleValue> values,
            ConcentrationUnit concentrationUnit
        ) {
            var percentiles = new List<(double, double)>();
            if (values != null) {
                var percentileValueRecords = values.Where(r => r.ValueType == ConcentrationValueType.Percentile).ToList();
                foreach (var record in percentileValueRecords) {
                    if (record.Percentile != null) {
                        var unitConversionFactor = record.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                        var value = unitConversionFactor * record.Value;
                        percentiles.Add(((double)record.Percentile, value));
                    }
                }
                if (values.Any(r => r.ValueType == ConcentrationValueType.MedianConcentration)) {
                    var record = values.First(r => r.ValueType == ConcentrationValueType.MedianConcentration);
                    var unitConversionFactor = record.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    var value = unitConversionFactor * record.Value;
                    percentiles.Add((50, value));
                }
            }
            return percentiles;
        }
    }
}
