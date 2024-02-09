using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureScenario : IStrongEntity {

        private string _name;

        public string Code { get; set; }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Code;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }
        public Population Population { get; set; }
        public ExposureType ExposureType { get; set; }
        public TargetLevelType ExposureLevel { get; set; }
        public string ExposureRoutes { get; set; }
        public ExternalExposureUnit ExposureUnit { get; set; }  
    }
}
