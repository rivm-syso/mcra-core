using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.DietaryExposureModels, typeof(RawDietaryExposureModel))]
    [RawTableObjectType(RawDataSourceTableID.DietaryExposurePercentiles, typeof(RawDietaryExposurePercentile))]
    [RawTableObjectType(RawDataSourceTableID.DietaryExposurePercentilesUncertain, typeof(RawDietaryExposurePercentileUncertain))]
    public sealed class RawDietaryExposuresData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.DietaryExposures;

        public override ActionType ActionType => ActionType.DietaryExposures;

        public List<RawDietaryExposureModel> DietaryExposureModelRecords { get; private set; }
        public List<RawDietaryExposurePercentile> DietaryExposurePercentileRecords { get; private set; }
        public List<RawDietaryExposurePercentileUncertain> DietaryExposurePercentileUncertainRecords { get; private set; }

        public RawDietaryExposuresData() : base() {
            DietaryExposureModelRecords = new List<RawDietaryExposureModel>();
            DataTables.Add(RawDataSourceTableID.DietaryExposureModels, new GenericRawDataTable<RawDietaryExposureModel>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposureModels,
                Records = DietaryExposureModelRecords
            });
            DietaryExposurePercentileRecords = new List<RawDietaryExposurePercentile>();
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentiles, new GenericRawDataTable<RawDietaryExposurePercentile>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentiles,
                Records = DietaryExposurePercentileRecords
            });
            DietaryExposurePercentileUncertainRecords = new List<RawDietaryExposurePercentileUncertain>();
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentilesUncertain, new GenericRawDataTable<RawDietaryExposurePercentileUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentilesUncertain,
                Records = DietaryExposurePercentileUncertainRecords
            });
        }
    }
}
