using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General.TableDefinitions.RawTableObjects;
namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.HbmSingleValueExposureSets, typeof(RawHbmSingleValueExposureSets))]
    [RawTableObjectType(RawDataSourceTableID.HbmSingleValueExposures, typeof(RawHbmSingleValueExposures))]
    public sealed class RawHbmSingleValueExposuresData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.HbmSingleValueExposures;

        public override ActionType ActionType => ActionType.HbmSingleValueExposures;

        public List<RawHbmSingleValueExposureSet> HbmSingleValueExposureSetRecords { get; private set; }
        public List<RawHbmSingleValueExposure> HbmSingleValueExposureRecords { get; private set; }

        public RawHbmSingleValueExposuresData() : base() {
            HbmSingleValueExposureSetRecords = [];
            DataTables.Add(RawDataSourceTableID.HbmSingleValueExposureSets, new GenericRawDataTable<RawHbmSingleValueExposureSet>() {
                RawDataSourceTableID = RawDataSourceTableID.HbmSingleValueExposureSets,
                Records = HbmSingleValueExposureSetRecords
            });
            HbmSingleValueExposureRecords = [];
            DataTables.Add(RawDataSourceTableID.HbmSingleValueExposures, new GenericRawDataTable<RawHbmSingleValueExposure>() {
                RawDataSourceTableID = RawDataSourceTableID.HbmSingleValueExposures,
                Records = HbmSingleValueExposureRecords
            });
        }
    }
}
