using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class UrineCorrectionCalculatorFactory {

        public static ICorrectionCalculator Create(
            StandardiseUrineMethod standardiseTotalLipidMethod,
            double specificGravityConversionFactor,
            List<string> substancesExcludedFromStandardisation
        ) {
            switch (standardiseTotalLipidMethod) {
                case StandardiseUrineMethod.SpecificGravity:
                    return new SpecificGravityCorrectionCalculator(substancesExcludedFromStandardisation);
                case StandardiseUrineMethod.CreatinineStandardisation:
                    return new CreatinineCorrectionCalculator(substancesExcludedFromStandardisation);
                case StandardiseUrineMethod.SpecificGravityCreatinineAdjustment:
                    return new SpecificGravityFromCreatinineCorrelationCalculator(substancesExcludedFromStandardisation, specificGravityConversionFactor);
                case StandardiseUrineMethod.SpecificGravityCreatinineNonlinearModelOne:
                    return new SpecificGravityFromCreatinineNonlinearModelOneCalculator(substancesExcludedFromStandardisation);
                case StandardiseUrineMethod.SpecificGravityCreatinineNonlinearModelTwo:
                    return new SpecificGravityFromCreatinineNonlinearModelTwoCalculator(substancesExcludedFromStandardisation);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
