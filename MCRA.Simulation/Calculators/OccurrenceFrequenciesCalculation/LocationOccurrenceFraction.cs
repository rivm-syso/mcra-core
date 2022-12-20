using MCRA.Data.Compiled.Wrappers.AgriculturalUseInfo;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class LocationOccurrenceFraction : ILocationOccurrenceFrequency {

        /// <summary>
        /// The location of the samples and for which the agricultural use is derived.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The fraction of samples that link to this location.
        /// </summary>
        public double FractionAllSamples { get; set; }

        /// <summary>
        /// Gets/sets whether agricultural use is allowed for this location.
        /// </summary>
        public bool SubstanceUseFound { get; set; }

        /// <summary>
        /// Gets/sets the agricultural use fraction of this location.
        /// </summary>
        public double OccurrenceFraction { get; set; }

        /// <summary>
        /// Gets/sets whether this location represents the undefined sample location
        /// for which only the general agricultural uses apply.
        /// </summary>
        public bool IsUndefinedLocation {
            get {
                return string.IsNullOrEmpty(Location);
            }
        }
    }
}
