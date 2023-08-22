using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class UrineCorrectionCalculatorFactory {

        public static IUrineCorrectionCalculator Create(
            StandardiseUrineMethod standardiseTotalLipidMethod,
            List<string> substancesExcludedFromStandardisation
        ) {
            switch (standardiseTotalLipidMethod) {
                case StandardiseUrineMethod.SpecificGravity:
                    return new SpecificGravityCorrectionCalculator(substancesExcludedFromStandardisation);
                case StandardiseUrineMethod.CreatinineStandardisation:
                    return new CreatinineCorrectionCalculator(substancesExcludedFromStandardisation);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
