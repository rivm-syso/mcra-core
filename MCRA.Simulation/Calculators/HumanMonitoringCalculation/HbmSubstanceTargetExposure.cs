using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmSubstanceTargetExposure : ISubstanceTargetExposureBase {

        /// <summary>
        /// The substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The target; for HBM internal, being a combination of biological matrix 
        /// and expression type.
        /// value applies.
        /// </summary>
        public ExposureTarget Target { get; set; }

        /// <summary>
        /// The estimate of the concentration at the target biological matrix obtained
        /// from human monitoring. Includes corrections for e.g., specific gravity.
        /// </summary>
        public double Exposure { get; set; }

        /// <summary>
        /// The original sampling methods of the from which this.
        /// </summary>
        public List<HumanMonitoringSamplingMethod> SourceSamplingMethods { get; set; }

        /// <summary>
        /// Specifies whether the record is an aggregate of multiple sampling methods.
        /// </summary>
        public bool IsAggregateOfMultipleSamplingMethods { get; set; }

        /// <summary>
        /// The total substance concentration corrected for RPF and 
        /// membership probability.
        /// </summary>
        public double EquivalentSubstanceExposure(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }

        /// <summary>
        /// Clones the substance target exposure record.
        /// </summary>
        /// <returns></returns>
        public HbmSubstanceTargetExposure Clone() {
            return new HbmSubstanceTargetExposure() {
                Substance = Substance,
                SourceSamplingMethods = SourceSamplingMethods,
                IsAggregateOfMultipleSamplingMethods = IsAggregateOfMultipleSamplingMethods,
                Exposure = Exposure
            };
        }
    }
}
