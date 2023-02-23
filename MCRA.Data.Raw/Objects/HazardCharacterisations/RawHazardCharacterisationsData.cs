using MCRA.General;

namespace MCRA.Data.Raw.Objects.HazardCharacterisations {

    [RawTableObjectType(RawDataSourceTableID.HazardCharacterisations, typeof(RawHazardCharacterisationRecord))]
    public sealed class RawHazardCharacterisationsData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.HazardCharacterisations;

        public List<RawHazardCharacterisationRecord> HazardCharacterisations { get; private set; }

        public RawHazardCharacterisationsData() : base() {
            HazardCharacterisations = new List<RawHazardCharacterisationRecord>();
            DataTables.Add(RawDataSourceTableID.HazardCharacterisations, new GenericRawDataTable<RawHazardCharacterisationRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.HazardCharacterisations,
                Records = HazardCharacterisations
            });
        }
    }
}
