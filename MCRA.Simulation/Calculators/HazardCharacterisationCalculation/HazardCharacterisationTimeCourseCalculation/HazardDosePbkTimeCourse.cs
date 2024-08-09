using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationTimeCourseCalculation {
    public class HazardDosePbkTimeCourse {
        public TargetUnit ExternalTargetUnit { get; set; }
        public TargetUnit InternalTargetUnit { get; set; }
        public KineticModelInstance KineticModelInstance { get; set; }
        public AggregateIndividualExposure AggregateIndividualExposure { get; set;}
        public IHazardCharacterisationModel HazardCharacterisation { get; set;}
    }
}
