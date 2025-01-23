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
            ProcessingFactorProvider processingFactorProvider
        ) {
            return dietaryDeterministicCalculationMethod switch {
                SingleValueDietaryExposuresCalculationMethod.IESTI => new IestiSingleValueDietaryExposureCalculator(processingFactorProvider, unitVariabilityDictionary, iestiSpecialCases, false),
                SingleValueDietaryExposuresCalculationMethod.IESTINew => new IestiSingleValueDietaryExposureCalculator(processingFactorProvider, unitVariabilityDictionary, null, true),
                SingleValueDietaryExposuresCalculationMethod.IEDI => new ChronicSingleValueDietaryExposureCalculator(processingFactorProvider, false),
                SingleValueDietaryExposuresCalculationMethod.TMDI => new ChronicSingleValueDietaryExposureCalculator(processingFactorProvider, true),
                SingleValueDietaryExposuresCalculationMethod.NEDI1 => new NediSingleValueDietaryExposureCalculator(processingFactorProvider, false),
                SingleValueDietaryExposuresCalculationMethod.NEDI2 => new NediSingleValueDietaryExposureCalculator(processingFactorProvider, true),
                _ => null,
            };
        }
    }
}
