using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            ICollection<KineticConversionFactor> kineticConversionFactors,
            TargetUnit targetUnit,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleTargetMatrixConversionCalculator(
                        conversionFactor,
                        targetUnit
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
