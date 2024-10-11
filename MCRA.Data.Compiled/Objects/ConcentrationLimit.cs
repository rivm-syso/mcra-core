using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationLimit {
        public Compound Compound { get; set; }
        public Food Food { get; set; }

        public double Limit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Reference { get; set; }

        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.mgPerKg;

        public ConcentrationLimitValueType ValueType { get; set; } = ConcentrationLimitValueType.MaximumResidueLimit;
    }
}
