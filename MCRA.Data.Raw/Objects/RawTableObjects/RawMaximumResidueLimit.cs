using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.MaximumResidueLimits)]
    public sealed class RawMaximumResidueLimit : IRawDataTableRecord {
        public string idFood { get; set; }
        public string idCompound { get; set; }
        public double Limit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ConcentrationUnit { get; set; }
        public string ValueType { get; set; }
        public string Reference { get; set; }
    }
}
