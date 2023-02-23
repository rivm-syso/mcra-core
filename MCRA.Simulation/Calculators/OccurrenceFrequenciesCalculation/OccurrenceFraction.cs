using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.AgriculturalUseInfo;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class OccurrenceFraction {

        /// <summary>
        /// The food to which the occurrence frequency applies.
        /// </summary>
        public Food Food{ get; set; }

        /// <summary>
        /// The substance to which the occurrence frequency applies.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The weigted agricultural use fraction of this compound.
        /// </summary>
        public double OccurrenceFrequency { get; set; }

        /// <summary>
        /// A dictionary containing the occurrence frequencies of this substance per location.
        /// The key is location.
        /// </summary>
        public Dictionary<string, ILocationOccurrenceFrequency> LocationOccurrenceFractions { get; set; }

        /// <summary>
        /// Returns the occurrence frequency for the specified location.
        /// If the location is null or cannot be found, the general occurrence
        /// frequency is returned.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public double GetOccurrenceFrequencyForLocation(string location) {
            if (LocationOccurrenceFractions.ContainsKey(location)) {
                return LocationOccurrenceFractions[location].OccurrenceFraction;
            } else {
                return LocationOccurrenceFractions[string.Empty].OccurrenceFraction;
            }
        }

        /// <summary>
        /// Returns if, according to the occurrence frequency, there is any occurrence for
        /// the specified location. If the location is null or cannot be found, the general
        /// location is returned.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool UseFound(string location) {
            if (LocationOccurrenceFractions.ContainsKey(location)) {
                return LocationOccurrenceFractions[location].SubstanceUseFound;
            } else {
                return LocationOccurrenceFractions[string.Empty].SubstanceUseFound;
            }
        }

        /// <summary>
        /// Returns all location specific occurrence frequencies.
        /// </summary>
        /// <returns></returns>
        public List<LocationOccurrenceFraction> GetAllLocationOccurrenceFrequencies() {
            return LocationOccurrenceFractions.Select(r => r.Value as LocationOccurrenceFraction).ToList();
        }

        public List<LocationOccurrenceFraction> GetSummarizedLocationAgriculturalUses() {
            var result = GetAllLocationOccurrenceFrequencies()
                .GroupBy(r => (Allowed: r.SubstanceUseFound, Fraction: r.OccurrenceFraction))
                .Select(g => new LocationOccurrenceFraction() {
                    FractionAllSamples = g.Sum(r => r.FractionAllSamples),
                    SubstanceUseFound = g.Key.Allowed,
                    OccurrenceFraction = g.Key.Fraction,
                    Location = g.Any(r => r.IsUndefinedLocation) ? string.Empty : string.Join(",", g.Select(r => r.Location)),
                })
                .ToList();
            return result;
        }
    }
}
