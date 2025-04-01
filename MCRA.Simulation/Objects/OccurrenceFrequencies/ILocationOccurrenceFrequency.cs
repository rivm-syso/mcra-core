namespace MCRA.Simulation.Objects {

    public interface ILocationOccurrenceFrequency {

        /// <summary>
        /// The location of the samples and for which the agricultural use is derived.
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// The fraction of samples that link to this location.
        /// </summary>
        double FractionAllSamples { get; set; }

        /// <summary>
        /// Gets/sets whether agricultural use is allowed for this location.
        /// </summary>
        bool SubstanceUseFound { get; set; }

        /// <summary>
        /// Gets/sets the agricultural use fraction of this location.
        /// </summary>
        double OccurrenceFraction { get; set; }

        /// <summary>
        /// Gets whether this location represents the undefined sample location
        /// for which only the general agricultural uses apply.
        /// </summary>
        bool IsUndefinedLocation { get; }

    }
}
