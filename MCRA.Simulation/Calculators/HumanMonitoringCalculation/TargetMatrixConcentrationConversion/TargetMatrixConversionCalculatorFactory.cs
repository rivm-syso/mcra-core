using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        /// <summary>
        /// Creates a new target matrix conversion calculator.
        /// </summary>
        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            TargetUnit targetUnit,
            ICollection<KineticConversionFactorModelBase> kineticConversionFactors,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleTargetMatrixConversionCalculator(
                        targetUnit,
                        conversionFactor
                    );
                case KineticConversionType.KineticConversion:
                    return new TargetMatrixKineticConversionCalculator(
                        kineticConversionFactors,
                        targetUnit
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
