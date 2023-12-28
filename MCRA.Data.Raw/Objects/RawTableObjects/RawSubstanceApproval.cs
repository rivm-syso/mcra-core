using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.SubstanceApprovals)]
    public sealed class RawSubstanceApproval : IRawDataTableRecord {
        public string idSubstance { get; set; }
        public bool IsApproved { get; set; }
    }
}
