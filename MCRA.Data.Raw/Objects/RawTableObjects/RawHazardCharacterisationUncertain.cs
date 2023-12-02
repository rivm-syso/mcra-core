using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.HazardCharacterisationsUncertain)]
    public sealed class RawHazardCharacterisationUncertain : IRawDataTableRecord {
        public string idHazardCharacterisation { get; set; }
        public string idSubstance { get; set; }
        public double Value { get; set; }

    }
}
