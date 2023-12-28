using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.Effects)]
    public sealed class RawEffect : IRawDataTableRecord {
        public string idEffect { get; set; }
        public string CodeSystem { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BiologicalOrganisation { get; set; }
        public string KeyEventProcess { get; set; }
        public string KeyEventObject { get; set; }
        public string KeyEventAction { get; set; }
        public string KeyEventOrgan { get; set; }
        public string KeyEventCell { get; set; }
        public string RiskType { get; set; }
        public string AOPWikiIds { get; set; }
        public string Reference { get; set; }
        public bool? IsDevelopmental { get; set; }
        public bool? IsGenotoxic { get; set; }
        public bool? IsAChEInhibitor { get; set; }
        public bool? IsNonGenotoxicCarcinogenic { get; set; }
    }
}
