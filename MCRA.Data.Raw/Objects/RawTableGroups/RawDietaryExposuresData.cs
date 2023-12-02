using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.DietaryExposureModels, typeof(RawDietaryExposureModelRecord))]
    [RawTableObjectType(RawDataSourceTableID.DietaryExposurePercentiles, typeof(RawDietaryExposurePercentileRecord))]
    [RawTableObjectType(RawDataSourceTableID.DietaryExposurePercentilesUncertain, typeof(RawDietaryExposurePercentileUncertainRecord))]
    public sealed class RawDietaryExposuresData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.DietaryExposures;

        public override ActionType ActionType => ActionType.DietaryExposures;

        public List<RawDietaryExposureModelRecord> DietaryExposureModelRecords { get; private set; }
        public List<RawDietaryExposurePercentileRecord> DietaryExposurePercentileRecords { get; private set; }
        public List<RawDietaryExposurePercentileUncertainRecord> DietaryExposurePercentileUncertainRecords { get; private set; }

        public RawDietaryExposuresData() : base() {
            DietaryExposureModelRecords = new List<RawDietaryExposureModelRecord>();
            DataTables.Add(RawDataSourceTableID.DietaryExposureModels, new GenericRawDataTable<RawDietaryExposureModelRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposureModels,
                Records = DietaryExposureModelRecords
            });
            DietaryExposurePercentileRecords = new List<RawDietaryExposurePercentileRecord>();
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentiles, new GenericRawDataTable<RawDietaryExposurePercentileRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentiles,
                Records = DietaryExposurePercentileRecords
            });
            DietaryExposurePercentileUncertainRecords = new List<RawDietaryExposurePercentileUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentilesUncertain, new GenericRawDataTable<RawDietaryExposurePercentileUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentilesUncertain,
                Records = DietaryExposurePercentileUncertainRecords
            });
        }
    }
}
