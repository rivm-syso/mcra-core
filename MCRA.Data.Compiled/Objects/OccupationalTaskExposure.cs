using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;

namespace MCRA.Data.Compiled.Objects {
    public sealed class OccupationalTaskExposure {
        public OccupationalTask OccupationalTask { get; set; }
        public RPEType RpeType { get; set; }
        public HandProtectionType HandProtectionType { get; set; }
        public ProtectiveClothingType ProtectiveClothingType { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public JobTaskExposureUnit Unit { get; set; }
        public JobTaskExposureEstimateType EstimateType { get; set; }
        public Compound Substance { get; set; }
        public double Percentage { get; set; }
        public double Value { get; set; }
        public string Reference { get; set; }

        public OccupationalTaskDeterminants Determinants() {
            return new OccupationalTaskDeterminants() {
                RPEType = RpeType,
                HandProtectionType = HandProtectionType,
                ProtectiveClothingType = ProtectiveClothingType
            };
        }
    }
}
