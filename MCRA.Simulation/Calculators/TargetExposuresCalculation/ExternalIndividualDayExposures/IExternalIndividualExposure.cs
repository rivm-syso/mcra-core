using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface IExternalIndividualExposure {
        Individual Individual { get; }
        double IndividualSamplingWeight { get; }
        int SimulatedIndividualId { get; }
        IDictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
        List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
