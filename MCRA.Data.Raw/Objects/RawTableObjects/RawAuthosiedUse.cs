using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.AuthorisedUses)]
    public sealed class RawAuthosiedUse : IRawDataTableRecord {
        public string idFood { get; set; }
        public string idSubstance { get; set; }
        public string Reference { get; set; }
    }
}
