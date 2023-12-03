using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.UnitVariabilityFactors)]
    public sealed class RawUnitVariabilityFactor : IRawDataTableRecord {
        public string idFood { get; set; }
        public string? idCompound { get; set; }
        public string? idProcessingType { get; set; }
        public double? Factor { get; set; }
        public double UnitsInCompositeSample { get; set; }
        public double? Coefficient { get; set; }
    }
}
