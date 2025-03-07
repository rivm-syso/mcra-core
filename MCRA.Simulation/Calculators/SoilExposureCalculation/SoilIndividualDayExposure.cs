using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public sealed class SoilIndividualDayExposure(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPathSubstance
    ) : ExternalIndividualDayExposure(exposuresPerPathSubstance) {

        public SoilIndividualDayExposure Clone() {
            return new SoilIndividualDayExposure(ExposuresPerPath) {
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                SimulatedIndividual = SimulatedIndividual,
                Day = Day
            };
        }
    }
}


