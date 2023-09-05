using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType,
            ICollection<KineticConversionFactor> kineticConversionFactors,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleTargetMatrixConversionCalculator(
                        conversionFactor
                    );
                case KineticConversionType.KineticConversion:
                    return new TargetMatrixKineticConversionCalculator(
                        kineticConversionFactors,
                        biologicalMatrix,
                        expressionType
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
