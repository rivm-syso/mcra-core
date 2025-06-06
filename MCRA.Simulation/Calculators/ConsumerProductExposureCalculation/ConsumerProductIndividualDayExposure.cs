using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductIndividualDayExposure(
            Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
        ) : ExternalIndividualDayExposure(exposuresPerPath) {
    }
}
