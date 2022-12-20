using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion {
    public interface IHazardDoseConverter {
        double GetExpressionTypeConversionFactor(PointOfDepartureType sourceType);
        double ConvertToTargetUnit(DoseUnit doseUnitSource, Compound compound, double dose);
    }
}
