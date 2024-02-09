using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ExposureDeterminantCombinations)]
    public sealed class RawExposureDeterminantCombination : IRawDataTableRecord {
        public string idExposureDeterminantCombination { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
