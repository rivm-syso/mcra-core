using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.RiskModels, typeof(RawRiskModel))]
    [RawTableObjectType(RawDataSourceTableID.RiskPercentiles, typeof(RawRiskPercentile))]
    [RawTableObjectType(RawDataSourceTableID.RiskPercentilesUncertain, typeof(RawRiskPercentileUncertain))]
    public sealed class RawRisksData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.Risks;

        public override ActionType ActionType => ActionType.Risks;

        public List<RawRiskModel> RiskModelRecords { get; private set; }
        public List<RawRiskPercentile> RiskPercentileRecords { get; private set; }
        public List<RawRiskPercentileUncertain> RiskPercentileUncertainRecords { get; private set; }

        public RawRisksData() : base() {
            RiskModelRecords = [];
            DataTables.Add(RawDataSourceTableID.RiskModels, new GenericRawDataTable<RawRiskModel>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskModels,
                Records = RiskModelRecords
            });
            RiskPercentileRecords = [];
            DataTables.Add(RawDataSourceTableID.RiskPercentiles, new GenericRawDataTable<RawRiskPercentile>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskPercentiles,
                Records = RiskPercentileRecords
            });
            RiskPercentileUncertainRecords = [];
            DataTables.Add(RawDataSourceTableID.RiskPercentilesUncertain, new GenericRawDataTable<RawRiskPercentileUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskPercentilesUncertain,
                Records = RiskPercentileUncertainRecords
            });
        }
    }
}
