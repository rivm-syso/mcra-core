using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PopulationConsumptionSingleValue {
        public Population Population { get; set; }
        public Food Food { get; set; }
        public double? Percentile { get; set; }
        public double ConsumptionAmount { get; set; }
        public string Reference { get; set; }
        public ConsumptionIntakeUnit ConsumptionUnit { get; set; } = ConsumptionIntakeUnit.gPerKgBWPerDay;
        public ConsumptionValueType ValueType { get; set; } = ConsumptionValueType.Undefined;
    }
}
