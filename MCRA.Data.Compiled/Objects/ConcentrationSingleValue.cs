using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationSingleValue {
        public Compound Substance { get; set; }
        public Food Food { get; set; }

        public double Value { get; set; }
        public double? Percentile { get; set; }

        public string Reference { get; set; }

        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.mgPerKg;
        public ConcentrationValueType ValueType { get; set; } = ConcentrationValueType.MeanConcentration;
    }
}
