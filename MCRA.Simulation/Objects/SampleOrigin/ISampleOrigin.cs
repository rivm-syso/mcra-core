using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects {
    public interface ISampleOrigin {

        /// <summary>
        /// THe food for which samples are recorded.
        /// </summary>
        Food Food { get; set; }

        /// <summary>
        /// The location from which samples are recorded.
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// The fraction of samples of the food that are from this location.
        /// </summary>
        float Fraction { get; set; }

        /// <summary>
        /// The number of samples of the food that are from this location.
        /// </summary>
        int NumberOfSamples { get; set; }

        /// <summary>
        /// Gets/sets whether this location represents the undefined sample location
        /// for which only the general agricultural uses apply.
        /// </summary>
        bool IsUndefinedLocation { get; }
    }
}
