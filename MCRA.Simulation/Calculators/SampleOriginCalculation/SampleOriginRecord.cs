using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.SampleOriginCalculation {
    public sealed class SampleOriginRecord : ISampleOrigin {

        /// <summary>
        /// THe food for which samples are recorded.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The location from which samples are recorded.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The fraction of samples of the food that are from this location.
        /// </summary>
        public float Fraction { get; set; }

        /// <summary>
        /// The number of samples of the food that are from this location.
        /// </summary>
        public int NumberOfSamples { get; set; }

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
