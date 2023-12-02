using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.HazardCharacterisations, typeof(RawHazardCharacterisationRecord))]
    [RawTableObjectType(RawDataSourceTableID.HazardCharacterisationsUncertain, typeof(RawHazardCharacterisationUncertainRecord))]
    public sealed class RawHazardCharacterisationsData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.HazardCharacterisations;

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public List<RawHazardCharacterisationRecord> HazardCharacterisations { get; private set; }

        public List<RawHazardCharacterisationUncertainRecord> HazardCharacterisationsUncertain { get; private set; }

        public RawHazardCharacterisationsData() : base() {
            HazardCharacterisations = new List<RawHazardCharacterisationRecord>();
            HazardCharacterisationsUncertain = new List<RawHazardCharacterisationUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.HazardCharacterisations, new GenericRawDataTable<RawHazardCharacterisationRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.HazardCharacterisations,
                Records = HazardCharacterisations
            });
            DataTables.Add(RawDataSourceTableID.HazardCharacterisationsUncertain, new GenericRawDataTable<RawHazardCharacterisationUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.HazardCharacterisationsUncertain,
                Records = HazardCharacterisationsUncertain
            });
        }
    }
}
