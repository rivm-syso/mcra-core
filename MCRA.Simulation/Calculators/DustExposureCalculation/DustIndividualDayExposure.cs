using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIndividualDayExposure(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : ExternalIndividualDayExposure(exposuresPerPath) {
    }
}


