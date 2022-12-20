using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Response : IStrongEntity {

        private string _name;

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

        public TestSystem TestSystem { get; set; }

        public string ResponseTypeString { get; set; }

        public string ResponseUnit { get; set; }

        public string GuidelineMethod { get; set; }

        public ResponseType ResponseType {
            get {
                return ResponseTypeConverter.FromString(ResponseTypeString);
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
