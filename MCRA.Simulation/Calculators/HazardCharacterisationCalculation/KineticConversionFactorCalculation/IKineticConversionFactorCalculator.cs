using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {
    public interface IKineticConversionFactorCalculator {

        double ComputeKineticConversionFactor(
            double dose,
            TargetUnit hazardDoseUnit,
            Compound substance,
            ExposureType exposureType,
            TargetUnit targetUnit,
            IRandom generator
        );
    }
}
