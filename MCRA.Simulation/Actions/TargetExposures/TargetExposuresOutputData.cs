using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {
    public class TargetExposuresOutputData : IModuleOutputData {
        public TargetUnit ExternalExposureUnit { get; set; }
        public TargetUnit TargetExposureUnit { get; set; }
        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures { get; set; }
        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }
        public IModuleOutputData Copy() {
            return new TargetExposuresOutputData() {
                ExternalExposureUnit = ExternalExposureUnit,
                TargetExposureUnit = TargetExposureUnit,
                AggregateIndividualDayExposures = AggregateIndividualDayExposures,
                AggregateIndividualExposures = AggregateIndividualExposures
            };
        }
    }
}

