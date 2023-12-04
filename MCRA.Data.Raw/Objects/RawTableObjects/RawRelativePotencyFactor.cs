using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.RelativePotencyFactors)]
    public sealed class RawRelativePotencyFactor : IRawDataTableRecord {
        public string idCompound { get; set; }
        public string idEffect { get; set; }
        public double RPF { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }
        public string Description { get; set; }
    }
}
