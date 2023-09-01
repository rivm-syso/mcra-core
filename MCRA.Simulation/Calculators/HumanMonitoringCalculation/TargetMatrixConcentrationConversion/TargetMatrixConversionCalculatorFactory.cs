using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            ICollection<KineticConversionFactor> kineticConversionFactors,
            BiologicalMatrix biologicalMatrix,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleTargetMatrixConversionCalculator(
                        conversionFactor,
                        biologicalMatrix
                    );
                case KineticConversionType.KineticConversion:
                    return new TargetMatrixKineticConversionCalculator(
                        kineticConversionFactors,
                        biologicalMatrix
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
