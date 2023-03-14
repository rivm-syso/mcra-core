using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.Compounds)]
    public class RawCompound : IRawDataTableRecord {
        public string idCompound { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcentrationUnit { get; set; }
        public double? ADI { get; set; }
        public double? ARFD { get; set; }
        public double? SF { get; set; }
        public int? CramerClass { get; set; }
        public double? MolecularMass { get; set; }
        public bool? IsLipidSoluble { get; set; }
    }
}
