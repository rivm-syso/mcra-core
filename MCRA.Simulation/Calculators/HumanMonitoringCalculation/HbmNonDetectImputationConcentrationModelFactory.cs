using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CensoredLogNormalImputationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectImputationCalculators;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmNonDetectImputationConcentrationModelFactory {

        public static IHbmNonDetectImputationConcentrationCalculator Create(
            IHbmIndividualDayConcentrationsCalculatorSettings settings
        ) {
            if (settings.NonDetectImputationMethod== NonDetectImputationMethod.ReplaceByLimit) {
                return new HbmNonDetectImputationCalculator(settings);
            } else {
                return new HbmNonDetectCensoredLognormalImputationCalculator(settings);
            }
        }
    }
}
