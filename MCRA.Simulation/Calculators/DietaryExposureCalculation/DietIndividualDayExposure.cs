using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DietExposureCalculation {

    public sealed class DietIndividualDayExposure(
            Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
        ) : ExternalIndividualDayExposure(exposuresPerPath) {

            public DietIndividualDayExposure Clone() {
            return new DietIndividualDayExposure(ExposuresPerPath) {
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                SimulatedIndividual = SimulatedIndividual,
                Day = Day,
            };
        }
    }
}


