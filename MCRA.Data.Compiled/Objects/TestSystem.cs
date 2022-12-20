using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class TestSystem : IStrongEntity {

        private string _name;
        private string _description;

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

        public string Description {
            get {
                if (string.IsNullOrEmpty(_description)) {
                    return Code;
                }
                return _description;
            }
            set {
                _description = value;
            }
        }

        public string Organ { get; set; }
        public string Species { get; set; }
        public string Strain { get; set; }
        public string TestSystemTypeString { get; set; }
        public string ExposureRouteTypeString { get; set; }
        public string GuidelineStudy { get; set; }
        public string Reference { get; set; }

        public TestSystemType TestSystemType {
            get {
                return TestSystemTypeConverter.FromString(TestSystemTypeString);
            }
        }

        public ExposureRouteType ExposureRouteType {
            get {
                if (TestSystemType != TestSystemType.InVivo) {
                    //if (string.IsNullOrEmpty(ExposureRouteTypeString) && TestSystemType != TestSystemType.InVivo) {
                        return ExposureRouteType.AtTarget;
                }
                return ExposureRouteTypeConverter.FromString(ExposureRouteTypeString);
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
