using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public interface IExternalIndividualExposure {
        SimulatedIndividual SimulatedIndividual { get; }
        Dictionary<ExposureRoute, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
        List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
