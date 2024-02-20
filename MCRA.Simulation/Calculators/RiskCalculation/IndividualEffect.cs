using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.RiskCalculation {

    /// <summary>
    /// A collection of data needed for health impact assessment: is initiated
    /// with exposure data and sampling weight to start with.
    /// Other quantities are added during the health effects modelling.
    /// </summary>
    public sealed class IndividualEffect : IIndividualEffect {

        /// <summary>
        /// Identifier of the (simulated) individual.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Reference to the individual.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// Risk expressed as the ratio hazard/exposure.
        /// </summary>
        public double HazardExposureRatio { get; set; }

        /// <summary>
        /// Risk expressed as the ratio exposure/hazard.
        /// </summary>
        public double ExposureHazardRatio { get; set; }

        /// <summary>
        /// Sampling weight of the individual.
        /// </summary>
        public double SamplingWeight { get; set; }

        /// <summary>
        /// The exposure estimate of the individual, can be a
        /// concentration (e.g., per kg BW) or an amount (per person).
        /// The unit is/should be aligned with the unit of the hazard
        /// characterisation (ced).
        /// </summary>
        public double Exposure { get; set; }

        /// <summary>
        /// Critical effect dose estimate of the individual. The exposure
        /// unit should be aligned with the unit of the hazard characterisation.
        /// </summary>
        public double CriticalEffectDose { get; set; }

        public double IntraSpeciesDraw { get; set; }

        public double PredictedHealthEffect { get; set; }

        public double EquivalentTestSystemDose { get; set; }

        /// <summary>
        /// The individual effect is calculated for
        /// 1) RPF weighted cumulative exposure
        /// 2) Sum of risk ratios, for the sum no exposure is available but contributing
        ///    substances may have exposure.
        ///    If all exposures by substance are zero, than IsPositive = false.
        /// 3) Individual substances
        /// </summary>
        public bool IsPositive { get; set; }
    }
}
