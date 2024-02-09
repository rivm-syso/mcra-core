using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ExposureScenarios)]
    public sealed class RawExposureScenario : IRawDataTableRecord {
        public string idExposureScenario { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idPopulation { get; set; }
        public string ExposureType { get; set; }
        public string ExposureLevel { get; set; }
        public string ExposureRoutes { get; set; }
        public string ExposureUnit { get; set; }
    }
}
