using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureScenario : StrongEntity {
        public Population Population { get; set; }
        public ExposureType ExposureType { get; set; }
        public TargetLevelType ExposureLevel { get; set; }
        public string ExposureRoutes { get; set; }
        public ExternalExposureUnit ExposureUnit { get; set; }  
    }
}
