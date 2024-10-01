using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.TargetExposureModels, typeof(RawTargetExposureModel))]
    [RawTableObjectType(RawDataSourceTableID.TargetExposurePercentiles, typeof(RawTargetExposurePercentile))]
    [RawTableObjectType(RawDataSourceTableID.TargetExposurePercentilesUncertain, typeof(RawTargetExposurePercentileUncertain))]
    public sealed class RawTargetExposuresData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.TargetExposures;

        public override ActionType ActionType => ActionType.TargetExposures;

        public List<RawTargetExposureModel> TargetExposureModelRecords { get; private set; }
        public List<RawTargetExposurePercentile> TargetExposurePercentileRecords { get; private set; }
        public List<RawTargetExposurePercentileUncertain> TargetExposurePercentileUncertainRecords { get; private set; }

        public RawTargetExposuresData() : base() {
            TargetExposureModelRecords = [];
            DataTables.Add(RawDataSourceTableID.TargetExposureModels, new GenericRawDataTable<RawTargetExposureModel>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposureModels,
                Records = TargetExposureModelRecords
            });
            TargetExposurePercentileRecords = [];
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentiles, new GenericRawDataTable<RawTargetExposurePercentile>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentiles,
                Records = TargetExposurePercentileRecords
            });
            TargetExposurePercentileUncertainRecords = [];
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentilesUncertain, new GenericRawDataTable<RawTargetExposurePercentileUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentilesUncertain,
                Records = TargetExposurePercentileUncertainRecords
            });
        }
    }
}
