using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public class SingleValueDietaryExposureCalculatorFactory {
        public static ISingleValueDietaryExposureCalculator Create(
            SingleValueDietaryExposuresCalculationMethod dietaryDeterministicCalculationMethod,
            Dictionary<Food, FoodUnitVariabilityInfo> unitVariabilityDictionary,
            ICollection<IestiSpecialCase> iestiSpecialCases,
            ProcessingFactorModelCollection processingFactors
        ) {
            switch (dietaryDeterministicCalculationMethod) {
                case SingleValueDietaryExposuresCalculationMethod.IESTI:
                    return new IestiSingleValueDietaryExposureCalculator(processingFactors, unitVariabilityDictionary, iestiSpecialCases, false);
                case SingleValueDietaryExposuresCalculationMethod.IESTINew:
                    return new IestiSingleValueDietaryExposureCalculator(processingFactors, unitVariabilityDictionary, null, true);
                case SingleValueDietaryExposuresCalculationMethod.IEDI:
                    return new ChronicSingleValueDietaryExposureCalculator(processingFactors, false);
                case SingleValueDietaryExposuresCalculationMethod.TMDI:
                    return new ChronicSingleValueDietaryExposureCalculator(processingFactors, true);
                case SingleValueDietaryExposuresCalculationMethod.NEDI1:
                    return new NediSingleValueDietaryExposureCalculator(processingFactors, false);
                case SingleValueDietaryExposuresCalculationMethod.NEDI2:
                    return new NediSingleValueDietaryExposureCalculator(processingFactors, true);
                default:
                    return null;
            }
        }
    }
}
