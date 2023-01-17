using MCRA.General;
using System;

namespace MCRA.Simulation.Calculators.RiskCalculation {

    /// <summary>
    /// A collection of data needed for health impact assessment: is initiated with exposure data and sampling weight to start with.
    /// Other quantities are added during the health effects modelling.
    /// </summary>
    public sealed class IndividualEffect : IIndividualEffect {


        public IndividualEffect() {

        }

        private const double _eps = 10E7D;
        private double _marginOfExposure = double.NaN;

        public int SimulationId { get; set; }


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

        public double RiskMetric(HealthEffectType healthEffectType, RiskMetricType riskMetric) {
            switch (riskMetric) {
                case RiskMetricType.MarginOfExposure:
                    return MarginOfExposure(healthEffectType);
                case RiskMetricType.HazardIndex:
                    return HazardIndex(healthEffectType);
                default:
                    throw new NotImplementedException($"Unknown risk metric type {riskMetric}.");
            }
        }

        public double MarginOfExposure(HealthEffectType healthEffectType) {
            if (double.IsNaN(_marginOfExposure)) {
                var iced = CriticalEffectDose;
                var iexp = ExposureConcentration;
                if (healthEffectType == HealthEffectType.Benefit) {
                    _marginOfExposure = iced > iexp / _eps ? iexp / iced : _eps;
                } else {
                    _marginOfExposure = iexp > iced / _eps ? iced / iexp : _eps;
                }
            }
            return _marginOfExposure;
        }

        public double HazardIndex(HealthEffectType healthEffectType) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return CriticalEffectDose / ExposureConcentration;
            } else {
                return ExposureConcentration / CriticalEffectDose;
            }
        }
    }
}
