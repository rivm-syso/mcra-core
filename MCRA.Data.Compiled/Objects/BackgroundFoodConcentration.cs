using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class BackgroundFoodConcentration {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public double Value { get; set; }
        public string Reference { get; set; }
        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.mgPerKg;
    }
}
