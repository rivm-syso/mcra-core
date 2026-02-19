using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public class ExternalOccupationalIndividualDayExposure : ExternalIndividualDayExposure {
        public ExternalOccupationalIndividualDayExposure(Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath) : base(exposuresPerPath) {
        }

        public OccupationalScenario OccupationalScenario { get; set; }
    }
}
