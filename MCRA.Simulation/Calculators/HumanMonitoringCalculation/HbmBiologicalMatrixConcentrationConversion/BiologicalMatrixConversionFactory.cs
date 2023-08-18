using System.Collections.Generic;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {
    public class BiologicalMatrixConversionFactory {

        public static IBiologicalMatrixConcentrationConversionCalculator Create(
            KineticConversionType kineticConversionType,
            ICollection<KineticConversionModel> kineticConversionFactors,
            BiologicalMatrix targetBiologicalMatrix,
            TargetUnitsModel targetUnitsModel,
            double conversionFactor
        ) {
            switch (kineticConversionType) {
                case KineticConversionType.Default:
                    return new SimpleBiologicalMatrixConcentrationConversionCalculator(conversionFactor);
                case KineticConversionType.KineticConversion:
                    return new ComplexBiologicalMatrixConcentrationConversionCalculator(
                        kineticConversionFactors,
                        targetBiologicalMatrix,
                        targetUnitsModel
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
