namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposuresPerIndividual {

        /// <summary>
        /// Body weight of the individual.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Peak exposure
        /// </summary>
        public double RelativeCompartmentWeight { get; set; } = 1D;

        /// <summary>
        /// The weight of the target compartment (e.g., liver weight).
        /// </summary>
        public double CompartmentWeight {
            get {
                return Weight * RelativeCompartmentWeight;
            }
        }

        /// <summary>
        /// Covariable of the individual.
        /// </summary>
        public double Covariable { get; set; }

        /// <summary>
        /// Cofactor of the individual.
        /// </summary>
        public string Cofactor { get; set; }

        /// <summary>
        /// Individual code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The external exposure.
        /// </summary>
        public double ExternalExposure { get; set; }


        public IDictionary<string, double> ExposurePerRoute { get; set; }
        /// <summary>
        /// Internal exposure (at target)
        /// </summary>
        public double TargetExposure { get; set; }

        /// <summary>
        /// Peak exposure
        /// </summary>
        public double PeakTargetExposure { get; set; }

        /// <summary>
        /// The absolute maximum of the of the target exposures time course.
        /// </summary>
        public double MaximumTargetExposure { get; set; }

        /// <summary>
        /// Peak exposure
        /// </summary>
        public double SteadyStateTargetExposure { get; set; }

        /// <summary>
        /// Time unit records.
        /// </summary>
        public List<TargetExposurePerTimeUnitRecord> TargetExposures { get; set; }

        public double ExternalExposurePerBodyweight {
            get {
                return ExternalExposure / Weight;
            }
        }

        public double InternalPeakTargetConcentration {
            get {
                return PeakTargetExposure / (Weight * RelativeCompartmentWeight);
            }
        }

        public double PeakAbsorptionFactor {
            get {
                return InternalPeakTargetConcentration / ExternalExposurePerBodyweight;
            }
        }

        public double InternalLongTermTargetConcentration {
            get {
                return SteadyStateTargetExposure / (Weight * RelativeCompartmentWeight);
            }
        }

        public double LongTermAbsorptionFactor {
            get {
                return InternalLongTermTargetConcentration / ExternalExposurePerBodyweight;
            }
        }
    }
}
