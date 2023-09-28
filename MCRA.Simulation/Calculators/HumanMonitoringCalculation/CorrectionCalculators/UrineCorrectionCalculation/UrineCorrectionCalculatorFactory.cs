using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class UrineCorrectionCalculatorFactory {

        public static ICorrectionCalculator Create(
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
