using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ExposureEstimates)]
    public sealed class RawExposureEstimate : IRawDataTableRecord {
        public string idExposureScenario { get; set; }
        public string idExposureDeterminantCombination { get; set; }
        public string ExposureSource { get; set; }
        public string idSubstance { get; set; }
        public string ExposureRoute { get; set; }
        public double Value { get; set; }
        public string EstimateType { get; set; }
    }
}
