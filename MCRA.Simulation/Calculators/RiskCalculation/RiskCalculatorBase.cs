using MCRA.General;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public abstract class RiskCalculatorBase {

        protected const double _eps = 10E7D;

        public HealthEffectType HealthEffectType { get; private set; }

        public RiskCalculatorBase(HealthEffectType healthEffectType) {
            HealthEffectType = healthEffectType;
        }

        protected double getHazardExposureRatio(
            HealthEffectType healthEffectType,
            double criticalEffectDose,
            double exposureConcentration
        ) {
            var iced = criticalEffectDose;
            var iexp = exposureConcentration;
            if (healthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / _eps ? iexp / iced : _eps;
            } else {
                return iexp > iced / _eps ? iced / iexp : _eps;
            }
        }

        protected double getExposureHazardRatio(
            HealthEffectType healthEffectType,
            double criticalEffectDose,
            double exposureConcentration
        ) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return criticalEffectDose / exposureConcentration;
            } else {
                return exposureConcentration / criticalEffectDose;
            }
        }
    }
}
