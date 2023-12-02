using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.TargetExposureModels)]
    public sealed class RawTargetExposureModelRecord : IRawDataTableRecord {
        public string idTargetExposureModel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idSubstance { get; set; }
        public string ExposureUnit { get; set; }
    }
}
