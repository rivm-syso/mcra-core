using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class UrineCorrectionCalculatorFactory {

        public static IUrineCorrectionCalculator Create(
            StandardiseUrineMethod standardiseTotalLipidMethod
        ) {
            switch (standardiseTotalLipidMethod) {
                case StandardiseUrineMethod.SpecificGravity:
                    return new SpecificGravityCorrectionCalculator();
                case StandardiseUrineMethod.CreatinineStandardisation:
                    return new CreatinineCorrectionCalculator();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
