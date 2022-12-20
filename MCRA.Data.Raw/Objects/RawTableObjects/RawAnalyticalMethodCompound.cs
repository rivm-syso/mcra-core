using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.AnalyticalMethodCompounds)]
    public class RawAnalyticalMethodCompound : IRawDataTableRecord {
        public string idAnalyticalMethod { get; set; }
        public string idCompound { get; set; }
        public double? LOD { get; set; }
        public double? LOQ { get; set; }
        public string ConcentrationUnit { get; set; }
    }
}
