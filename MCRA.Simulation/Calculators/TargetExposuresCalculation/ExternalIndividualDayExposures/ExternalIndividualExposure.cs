using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class ExternalIndividualExposure : IExternalIndividualExposure {
        public Individual Individual { get; set; }
        public double IndividualSamplingWeight { get; set; }
        public int SimulatedIndividualId { get; set; }
        public IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
