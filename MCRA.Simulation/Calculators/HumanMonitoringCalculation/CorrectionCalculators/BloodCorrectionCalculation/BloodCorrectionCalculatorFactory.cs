using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.BloodCorrectionCalculation {
    public class BloodCorrectionCalculatorFactory {

        public static ICorrectionCalculator Create(
            StandardiseBloodMethod standardiseTotalLipidMethod,
            List<string> substancesExcludedFromStandardisation
        ) {
            return standardiseTotalLipidMethod switch {
                StandardiseBloodMethod.GravimetricAnalysis => new LipidGravimetricCorrectionCalculator(substancesExcludedFromStandardisation),
                StandardiseBloodMethod.EnzymaticSummation => new LipidEnzymaticCorrectionCalculator(substancesExcludedFromStandardisation),
                StandardiseBloodMethod.BernertMethod => new LipidBernertCorrectionCalculator(substancesExcludedFromStandardisation),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
