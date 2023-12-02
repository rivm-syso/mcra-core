using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.TargetExposureModels, typeof(RawTargetExposureModelRecord))]
    [RawTableObjectType(RawDataSourceTableID.TargetExposurePercentiles, typeof(RawTargetExposurePercentileRecord))]
    [RawTableObjectType(RawDataSourceTableID.TargetExposurePercentilesUncertain, typeof(RawTargetExposurePercentileUncertainRecord))]
    public sealed class RawTargetExposuresData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.TargetExposures;

        public override ActionType ActionType => ActionType.TargetExposures;

        public List<RawTargetExposureModelRecord> TargetExposureModelRecords { get; private set; }
        public List<RawTargetExposurePercentileRecord> TargetExposurePercentileRecords { get; private set; }
        public List<RawTargetExposurePercentileUncertainRecord> TargetExposurePercentileUncertainRecords { get; private set; }

        public RawTargetExposuresData() : base() {
            TargetExposureModelRecords = new List<RawTargetExposureModelRecord>();
            DataTables.Add(RawDataSourceTableID.TargetExposureModels, new GenericRawDataTable<RawTargetExposureModelRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposureModels,
                Records = TargetExposureModelRecords
            });
            TargetExposurePercentileRecords = new List<RawTargetExposurePercentileRecord>();
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentiles, new GenericRawDataTable<RawTargetExposurePercentileRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentiles,
                Records = TargetExposurePercentileRecords
            });
            TargetExposurePercentileUncertainRecords = new List<RawTargetExposurePercentileUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.TargetExposurePercentilesUncertain, new GenericRawDataTable<RawTargetExposurePercentileUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.TargetExposurePercentilesUncertain,
                Records = TargetExposurePercentileUncertainRecords
            });
        }
    }
}
