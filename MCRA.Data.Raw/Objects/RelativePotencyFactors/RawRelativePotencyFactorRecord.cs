using MCRA.General;

namespace MCRA.Data.Raw.Objects.RelativePotencyFactors {
    [RawDataSourceTableID(RawDataSourceTableID.RelativePotencyFactors)]
    public sealed class RawRelativePotencyFactorRecord : IRawDataTableRecord {
        public string idCompound { get; set; }
        public string idEffect { get; set; }
        public double RPF { get; set; }
    }
}
