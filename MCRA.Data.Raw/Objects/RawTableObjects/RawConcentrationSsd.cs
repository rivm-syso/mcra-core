using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.ConcentrationsSSD)]
    public class RawConcentrationSsd : IRawDataTableRecord {
        public string labSampCode { get; set; }
        public string labSubSampCode { get; set; }
        public string sampCountry { get; set; }
        public string sampArea { get; set; }
        public string prodCode { get; set; }
        public string prodProdMeth { get; set; }
        public int sampY { get; set; }
        public int? sampM { get; set; }
        public int? sampD { get; set; }
        public int analysisY { get; set; }
        public int? analysisM { get; set; }
        public int? analysisD { get; set; }
        public string paramCode { get; set; }
        public string resUnit { get; set; }
        public double? resLOD { get; set; }
        public double? resLOQ { get; set; }
        public double? resVal { get; set; }
        public string resType { get; set; }
    }
}
