using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class UrineCorrectionCalculatorFactory {

        public static ICorrectionCalculator Create(
            StandardiseUrineMethod standardiseTotalLipidMethod,
            double specificGravityConversionFactor,
            List<string> substancesExcludedFromStandardisation
        ) {
            return standardiseTotalLipidMethod switch {
                StandardiseUrineMethod.SpecificGravity => new SpecificGravityCorrectionCalculator(substancesExcludedFromStandardisation),
                StandardiseUrineMethod.CreatinineStandardisation => new CreatinineCorrectionCalculator(substancesExcludedFromStandardisation),
                StandardiseUrineMethod.SpecificGravityCreatinineAdjustment => new SpecificGravityFromCreatinineCorrelationCalculator(substancesExcludedFromStandardisation, specificGravityConversionFactor),
                StandardiseUrineMethod.SpecificGravityCreatinineNonlinearModelOne => new SpecificGravityFromCreatinineNonlinearModelOneCalculator(substancesExcludedFromStandardisation),
                StandardiseUrineMethod.SpecificGravityCreatinineNonlinearModelTwo => new SpecificGravityFromCreatinineNonlinearModelTwoCalculator(substancesExcludedFromStandardisation),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
