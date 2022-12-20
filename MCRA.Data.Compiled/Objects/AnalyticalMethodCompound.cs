using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class AnalyticalMethodCompound {
        public Compound Compound { get; set; }
        public AnalyticalMethod AnalyticalMethod { get; set; }
        public double LOR {
            get {
                return !double.IsNaN(LOQ) ? LOQ : LOD;
            }
        }
        public double LOD { get; set; }
        public double LOQ { get; set; }
        public string ConcentrationUnitString { get; set; }

        public ConcentrationUnit GetConcentrationUnit() {
            if (!string.IsNullOrEmpty(ConcentrationUnitString)) {
                return ConcentrationUnitConverter.FromString(ConcentrationUnitString);
            } else {
                return ConcentrationUnit.mgPerKg;
            }
        }
    }
}
