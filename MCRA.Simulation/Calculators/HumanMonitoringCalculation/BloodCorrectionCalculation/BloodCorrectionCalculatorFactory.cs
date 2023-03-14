using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public class BloodCorrectionCalculatorFactory {

        public static IBloodCorrectionCalculator Create(
            StandardiseBloodMethod standardiseTotalLipidMethod
        ) {
            switch (standardiseTotalLipidMethod) {
                case StandardiseBloodMethod.GravimetricAnalysis:
                    return new LipidGravimetricCorrectionCalculator();
                case StandardiseBloodMethod.EnzymaticSummation:
                    return new LipidEnzymaticCorrectionCalculator();
                case StandardiseBloodMethod.BernertMethod:
                    return new LipidBernertCorrectionCalculator();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
