using MCRA.General;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public sealed class ExternalExposureCollection {

        /// <summary>
        /// The exposure unit.
        /// </summary>
        public ExposureUnitTriple ExposureUnit { get; set; }

        /// <summary>
        /// Exposure source of this collection.
        /// </summary>
        public ExposureSource ExposureSource { get; set; }

        /// <summary>
        /// The individual external exposures.
        /// </summary>
        public ICollection<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }

    }
}
