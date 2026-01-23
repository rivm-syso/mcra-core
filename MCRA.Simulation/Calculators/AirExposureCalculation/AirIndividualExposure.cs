using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {
    public sealed class AirIndividualExposure(
        SimulatedIndividual individual,
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : ExternalIndividualExposure(individual, exposuresPerPath) {
    }
}
