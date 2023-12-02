using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.RelativePotencyFactors)]
    public sealed class RawRelativePotencyFactor : IRawDataTableRecord {
        public string idCompound { get; set; }
        public string idEffect { get; set; }
        public double RPF { get; set; }
    }
}
