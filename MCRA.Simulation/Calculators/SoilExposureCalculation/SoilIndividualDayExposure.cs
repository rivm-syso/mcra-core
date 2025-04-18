using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public sealed class SoilIndividualDayExposure(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPathSubstance
    ) : ExternalIndividualDayExposure(exposuresPerPathSubstance) {

        public SoilIndividualDayExposure Clone(IIndividualDay individualDay) {
            return new SoilIndividualDayExposure(ExposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day,
            };
        }
    }
}


