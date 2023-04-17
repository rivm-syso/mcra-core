using MCRA.General;

namespace MCRA.Simulation.Calculators.RiskCalculation {

    /// <summary>
    /// A collection of data needed for health impact assessment: is initiated with exposure data and sampling weight to start with.
    /// Other quantities are added during the health effects modelling.
    /// </summary>
    public sealed class IndividualEffect : IIndividualEffect {

        public IndividualEffect() {

        }

        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// - Amount per person or per kg bodyweight
        /// - Concentration
        /// </summary>
        public double ExposureConcentration { get; set; }
        public double CriticalEffectDose { get; set; }
        public double SamplingWeight { get; set; }
        public double PredictedHealthEffect { get; set; }
        public double EquivalentTestSystemDose { get; set; }

        public double CompartmentWeight { get; set; }
        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Is needed for calculation based on inverse HazardIndex
        /// </summary>
        public double MarginOfExposure { get; set; }
        /// <summary>
        /// Is needed for calculation based on inverse MarginOfExposure
        /// </summary>
        public double HazardIndex { get; set; }

        /// <summary>
        /// The individual effect is calculated for
        /// 1) RPF weighted cumulative exposure
        /// 2) Sum of risk ratios, for the sum no exposure is available but contributing substances may have exposure. 
        ///    If all exposures by substance are zero, than IsPositive = false.
        /// 3) Individual substances
        /// </summary>
        public bool IsPositive { get; set; }
    }
}
