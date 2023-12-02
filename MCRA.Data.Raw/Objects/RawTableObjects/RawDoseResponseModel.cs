using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DoseResponseModels)]
    public sealed class RawDoseResponseModel : IRawDataTableRecord {
        public string idExperiment { get; set; }
        public string idDoseResponseModel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idResponse { get; set; }
        public string Substances { get; set; }
        public string Covariates { get; set; }
        public double CriticalEffectSize { get; set; }
        public DoseResponseModelType DoseResponseModelType { get; set; }
        public BenchmarkResponseType BenchmarkResponseType { get; set; }
        public string ModelEquation { get; set; }
        public double? LogLikelihood { get; set; }
        public string DoseUnit { get; set; }
        public string ProastVersion { get; set; }
        // Field deprecated
        public string ModelParameterValues { get; set; }
    }
}
