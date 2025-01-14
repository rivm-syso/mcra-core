using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public sealed class ExternalIndividualExposure(SimulatedIndividual individual) : IExternalIndividualExposure {
        public SimulatedIndividual SimulatedIndividual { get; } = individual;
        public Dictionary<ExposureRoute, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
