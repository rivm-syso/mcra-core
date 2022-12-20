using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationPerSample {
        public SampleAnalysis Sample { get; set; }
        public Compound Compound { get; set; }
        public double? Concentration { get; set; }

        public ResType ResType {
            get {
                if (!string.IsNullOrEmpty(ResTypeString)) {
                    return ResTypeConverter.FromString(ResTypeString);
                }
                return ResType.VAL;
            }
        }

        public string ResTypeString { get; set; }
    }
}
