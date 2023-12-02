using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

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
            TargetExposureModelRecords = new List<RawTargetExposureModel>();
            DataTables.Add(RawDataSourceTableID.TargetExposureModels, new GenericRawDataTable<RawTargetExposureModel>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposureModels,
                Records = TargetExposureModelRecords
            });
            TargetExposurePercentileRecords = new List<RawTargetExposurePercentile>();
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentiles, new GenericRawDataTable<RawTargetExposurePercentile>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentiles,
                Records = TargetExposurePercentileRecords
            });
            TargetExposurePercentileUncertainRecords = new List<RawTargetExposurePercentileUncertain>();
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentilesUncertain, new GenericRawDataTable<RawTargetExposurePercentileUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentilesUncertain,
                Records = TargetExposurePercentileUncertainRecords
            });
        }
    }
}
