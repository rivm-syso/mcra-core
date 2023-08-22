using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public class BloodCorrectionCalculatorFactory {

        public static IBloodCorrectionCalculator Create(
            StandardiseBloodMethod standardiseTotalLipidMethod,
            List<string> substancesExcludedFromStandardisation
        ) {
            switch (standardiseTotalLipidMethod) {
                case StandardiseBloodMethod.GravimetricAnalysis:
                    return new LipidGravimetricCorrectionCalculator(substancesExcludedFromStandardisation);
                case StandardiseBloodMethod.EnzymaticSummation:
                    return new LipidEnzymaticCorrectionCalculator(substancesExcludedFromStandardisation);
                case StandardiseBloodMethod.BernertMethod:
                    return new LipidBernertCorrectionCalculator(substancesExcludedFromStandardisation);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
