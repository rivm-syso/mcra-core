using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.RelativePotencyFactorsUncertain)]
    public sealed class RawRelativePotencyFactorUncertainRecord : IRawDataTableRecord {
        public string idCompound { get; set; }
        public string idEffect { get; set; }
        public string idUncertaintySet { get; set; }
        public double RPF { get; set; }
    }
}
