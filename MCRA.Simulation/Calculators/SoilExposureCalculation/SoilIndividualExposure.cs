using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public sealed class SoilIndividualExposure(
        SimulatedIndividual individual,
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPathSubstance
    ) : ExternalIndividualExposure(individual, exposuresPerPathSubstance) {
    }
}


