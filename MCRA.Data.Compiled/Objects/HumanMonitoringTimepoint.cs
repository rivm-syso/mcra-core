namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringTimepoint : IStrongEntity {

        private string _name;

        public HumanMonitoringTimepoint() {
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

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
