using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

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
            DietaryExposureModelRecords = [];
            DataTables.Add(RawDataSourceTableID.DietaryExposureModels, new GenericRawDataTable<RawDietaryExposureModel>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposureModels,
                Records = DietaryExposureModelRecords
            });
            DietaryExposurePercentileRecords = [];
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentiles, new GenericRawDataTable<RawDietaryExposurePercentile>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentiles,
                Records = DietaryExposurePercentileRecords
            });
            DietaryExposurePercentileUncertainRecords = [];
            DataTables.Add(RawDataSourceTableID.DietaryExposurePercentilesUncertain, new GenericRawDataTable<RawDietaryExposurePercentileUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.DietaryExposurePercentilesUncertain,
                Records = DietaryExposurePercentileUncertainRecords
            });
        }
    }
}
