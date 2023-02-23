namespace MCRA.Data.Compiled.Objects {
    public sealed class AnalyticalMethod: IStrongEntity {
        private string _name;
        public AnalyticalMethod() {
            AnalyticalMethodCompounds = new Dictionary<Compound, AnalyticalMethodCompound>();
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
        public int SampleCount { get; set; }

        public IDictionary<Compound, AnalyticalMethodCompound> AnalyticalMethodCompounds { get; set; }

        public override string ToString() => $"[{GetHashCode():X8}] {Code}";
    }
}
