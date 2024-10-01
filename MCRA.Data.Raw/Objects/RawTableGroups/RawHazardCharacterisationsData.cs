using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.HazardCharacterisations, typeof(RawHazardCharacterisation))]
    [RawTableObjectType(RawDataSourceTableID.HazardCharacterisationsUncertain, typeof(RawHazardCharacterisationUncertain))]
    public sealed class RawHazardCharacterisationsData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.HazardCharacterisations;

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public List<RawHazardCharacterisation> HazardCharacterisations { get; private set; }

        public List<RawHazardCharacterisationUncertain> HazardCharacterisationsUncertain { get; private set; }

        public RawHazardCharacterisationsData() : base() {
            HazardCharacterisations = [];
            HazardCharacterisationsUncertain = [];
            DataTables.Add(RawDataSourceTableID.HazardCharacterisations, new GenericRawDataTable<RawHazardCharacterisation>() {
                RawDataSourceTableID = RawDataSourceTableID.HazardCharacterisations,
                Records = HazardCharacterisations
            });
            DataTables.Add(RawDataSourceTableID.HazardCharacterisationsUncertain, new GenericRawDataTable<RawHazardCharacterisationUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.HazardCharacterisationsUncertain,
                Records = HazardCharacterisationsUncertain
            });
        }
    }
}
