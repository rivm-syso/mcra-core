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
            return dietaryDeterministicCalculationMethod switch {
                SingleValueDietaryExposuresCalculationMethod.IESTI => new IestiSingleValueDietaryExposureCalculator(processingFactors, unitVariabilityDictionary, iestiSpecialCases, false),
                SingleValueDietaryExposuresCalculationMethod.IESTINew => new IestiSingleValueDietaryExposureCalculator(processingFactors, unitVariabilityDictionary, null, true),
                SingleValueDietaryExposuresCalculationMethod.IEDI => new ChronicSingleValueDietaryExposureCalculator(processingFactors, false),
                SingleValueDietaryExposuresCalculationMethod.TMDI => new ChronicSingleValueDietaryExposureCalculator(processingFactors, true),
                SingleValueDietaryExposuresCalculationMethod.NEDI1 => new NediSingleValueDietaryExposureCalculator(processingFactors, false),
                SingleValueDietaryExposuresCalculationMethod.NEDI2 => new NediSingleValueDietaryExposureCalculator(processingFactors, true),
                _ => null,
            };
        }
    }
}
