using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public interface IExternalIndividualExposure {
        int SimulatedIndividualId { get; }
        double IndividualSamplingWeight { get; }
        Individual Individual { get; }
        Dictionary<ExposureRoute, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
        List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
