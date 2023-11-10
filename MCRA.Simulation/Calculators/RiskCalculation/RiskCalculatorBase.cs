using MCRA.General;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public abstract class RiskCalculatorBase {

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
                return iced > iexp / SimulationConstants.MOE_eps ? iexp / iced : SimulationConstants.MOE_eps;
            } else {
                return iexp > iced / SimulationConstants.MOE_eps ? iced / iexp : SimulationConstants.MOE_eps;
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
