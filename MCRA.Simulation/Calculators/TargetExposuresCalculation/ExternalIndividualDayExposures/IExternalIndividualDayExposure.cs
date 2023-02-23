using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface IExternalIndividualDayExposure {
        int SimulatedIndividualId { get; }
        int SimulatedIndividualDayId { get; }
        Individual Individual { get; }
        string Day { get; }
        double IndividualSamplingWeight { get; set; }
        IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
    }
}
