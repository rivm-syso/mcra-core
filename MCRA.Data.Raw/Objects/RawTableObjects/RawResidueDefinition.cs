using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ResidueDefinitions)]
    public sealed class RawResidueDefinition : IRawDataTableRecord {
        public string idMeasuredSubstance { get; set; }
        public string idActiveSubstance { get; set; }
        public double ConversionFactor { get; set; }
        public bool IsExclusive { get; set; }
        public double? Proportion { get; set; }
    }
}
