using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmSubstanceTargetExposure : IHbmSubstanceTargetExposure {

        /// <summary>
        /// The substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The biological matrix for which this concentration
        /// value applies.
        /// </summary>
        public BiologicalMatrix BiologicalMatrix { get; set; }

        /// <summary>
        /// The estimate of the concentration at the target biological matrix obtained
        /// from human monitoring. Includes corrections for e.g., specific gravity.
        /// </summary>
        public double Concentration { get; set; }

        /// <summary>
        /// The original sampling methods of the from which this.
        /// </summary>
        public List<HumanMonitoringSamplingMethod> SourceSamplingMethods { get; set; }

        /// <summary>
        /// Returns whether this concentration value is derived from a concentration
        /// measurement in another biological matrix.
        /// </summary>
        public bool IsDerivedFromOtherMatrix {
            get {
                if (SourceSamplingMethods?.Any() ?? false) {
                    var originalMatrices = SourceSamplingMethods
                        .Select(r => r.BiologicalMatrix)
                        .Distinct()
                        .ToList();
                    if (originalMatrices.Count > 1) {
                        return true;
                    } else {
                        return originalMatrices.First() != BiologicalMatrix;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Specifies whether the record is an aggregate of multiple sampling methods.
        /// </summary>
        public bool IsAggregateOfMultipleSamplingMethods { get; set; }

        /// <summary>
        /// The total substance concentration corrected for RPF and 
        /// membership probability.
        /// </summary>
        public double EquivalentSubstanceConcentration(double rpf, double membershipProbability) {
            return Concentration * rpf * membershipProbability;
        }
    }
}
