using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class TargetMatrixConversionCalculatorFactory {

        public static ITargetMatrixConversionCalculator Create(
            KineticConversionType kineticConversionType,
            TargetUnit targetUnit,
            //KineticConversionFactorModel, voor nominal constant model, uncertainty die andere
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
