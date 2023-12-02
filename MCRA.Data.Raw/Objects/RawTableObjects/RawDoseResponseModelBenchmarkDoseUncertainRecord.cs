using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain)]
    public sealed class RawDoseResponseModelBenchmarkDoseUncertainRecord : IRawDataTableRecord {
        public string idDoseResponseModel { get; set; }
        public string idSubstance { get; set; }
        public string idUncertaintySet { get; set; }
        public string Covariates { get; set; }
        public double BenchmarkDose { get; set; }
        public double Rpf { get; set; }
    }
}
