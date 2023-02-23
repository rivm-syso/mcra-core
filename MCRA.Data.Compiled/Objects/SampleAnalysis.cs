namespace MCRA.Data.Compiled.Objects {
    public sealed class SampleAnalysis {
        private string _name;

        public SampleAnalysis() {
            Concentrations = new Dictionary<Compound, ConcentrationPerSample>();
        }
        public string Code { get; set; }
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Code;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string Description { get; set; }
        public DateTime? AnalysisDate { get; set; }
        public AnalyticalMethod AnalyticalMethod { get; set; }
        public IDictionary<Compound, ConcentrationPerSample> Concentrations { get; set; }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
