namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryExposureSource : IStrongEntity {

        private string _name;

        public NonDietaryExposureSource() {
        }

        public NonDietaryExposureSource(string code) : this() {
            Code = code;
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
