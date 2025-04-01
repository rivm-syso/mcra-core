using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class OccurrenceFractionsBuilder {

        private readonly IOccurrenceFractionsCalculatorSettings _settings;

        public OccurrenceFractionsBuilder(
            IOccurrenceFractionsCalculatorSettings settings
        ) {
            _settings = settings;
        }

        /// <summary>
        /// Creates occurrence fractions from the collection of occurrency frequencies.
        /// </summary>
        /// <param name="occurrenceFrequencies"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), OccurrenceFraction> Create(
            ICollection<OccurrenceFrequency> occurrenceFrequencies
        ) {
            var result = new Dictionary<(Food, Compound), OccurrenceFraction>();
            foreach (var occurrenceFrequency in occurrenceFrequencies) {
                var fraction = _settings.UseAgriculturalUsePercentage
                    ? occurrenceFrequency.Percentage / 100D
                    : (occurrenceFrequency.Percentage > 0 ? 1D : 0D);
                var record = createOccurrenceFraction(occurrenceFrequency, fraction);
                result.Add((occurrenceFrequency.Food, occurrenceFrequency.Substance), record);
            }
            return result;
        }

        private static OccurrenceFraction createOccurrenceFraction(OccurrenceFrequency occurrenceFrequency, double fraction) {
            var locationUseRecord = new LocationOccurrenceFraction() {
                Location = string.Empty,
                FractionAllSamples = 1D,
                SubstanceUseFound = fraction > 0,
                OccurrenceFraction = fraction
            };
            var record = new OccurrenceFraction() {
                Food = occurrenceFrequency.Food,
                Substance = occurrenceFrequency.Substance,
                OccurrenceFrequency = fraction,
                LocationOccurrenceFractions = new Dictionary<string, ILocationOccurrenceFrequency> {
                    { string.Empty, locationUseRecord }
                }
            };
            return record;
        }
    }
}
