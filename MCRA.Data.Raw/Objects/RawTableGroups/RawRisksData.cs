using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.RiskModels, typeof(RawRiskModelRecord))]
    [RawTableObjectType(RawDataSourceTableID.RiskPercentiles, typeof(RawRiskPercentileRecord))]
    [RawTableObjectType(RawDataSourceTableID.RiskPercentilesUncertain, typeof(RawRiskPercentileUncertainRecord))]
    public sealed class RawRisksData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.Risks;

        public override ActionType ActionType => ActionType.Risks;

        public List<RawRiskModelRecord> RiskModelRecords { get; private set; }
        public List<RawRiskPercentileRecord> RiskPercentileRecords { get; private set; }
        public List<RawRiskPercentileUncertainRecord> RiskPercentileUncertainRecords { get; private set; }

        public RawRisksData() : base() {
            RiskModelRecords = new List<RawRiskModelRecord>();
            DataTables.Add(RawDataSourceTableID.RiskModels, new GenericRawDataTable<RawRiskModelRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskModels,
                Records = RiskModelRecords
            });
            RiskPercentileRecords = new List<RawRiskPercentileRecord>();
            DataTables.Add(RawDataSourceTableID.RiskPercentiles, new GenericRawDataTable<RawRiskPercentileRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskPercentiles,
                Records = RiskPercentileRecords
            });
            RiskPercentileUncertainRecords = new List<RawRiskPercentileUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.RiskPercentilesUncertain, new GenericRawDataTable<RawRiskPercentileUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.RiskPercentilesUncertain,
                Records = RiskPercentileUncertainRecords
            });
        }
    }
}
