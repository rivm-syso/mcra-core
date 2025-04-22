using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {

    public sealed class AirIndividualDayExposure(
            Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
        ) : ExternalIndividualDayExposure(exposuresPerPath) {
    }
}
