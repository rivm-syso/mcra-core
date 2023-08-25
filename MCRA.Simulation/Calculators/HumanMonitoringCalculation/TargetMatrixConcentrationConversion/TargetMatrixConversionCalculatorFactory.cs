using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            ICollection<KineticConversionFactor> kineticConversionFactors,
            List<TargetUnit> targetUnits,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleTargetMatrixConversionCalculator(
                        conversionFactor,
                        targetUnits.FirstOrDefault().BiologicalMatrix
                    );
                case KineticConversionType.KineticConversion:
                    return new TargetMatrixKineticConversionCalculator(
                        kineticConversionFactors,
                        targetUnits.FirstOrDefault()
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
