using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIndividualExposure(
        SimulatedIndividual individual,
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : ExternalIndividualExposure(individual, exposuresPerPath) {
    }
}


