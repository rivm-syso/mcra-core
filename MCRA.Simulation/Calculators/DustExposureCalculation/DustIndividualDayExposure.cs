using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIndividualDayExposure(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : ExternalIndividualDayExposure(exposuresPerPath) {

        public DustIndividualDayExposure Clone() {
            return new DustIndividualDayExposure(ExposuresPerPath) {
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                SimulatedIndividual = SimulatedIndividual,
                Day = Day
            };
        }
    }
}


