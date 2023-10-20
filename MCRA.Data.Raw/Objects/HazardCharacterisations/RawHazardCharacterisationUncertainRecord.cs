using MCRA.General;

namespace MCRA.Data.Raw.Objects.HazardCharacterisations {

    [RawDataSourceTableID(RawDataSourceTableID.HazardCharacterisationsUncertain)]
    public sealed class RawHazardCharacterisationUncertainRecord : IRawDataTableRecord {
        public string idHazardCharacterisation { get; set; }
        public string idSubstance { get; set; }
        public double Value { get; set; }
       
    }
}
