using MCRA.General;

namespace MCRA.Data.Raw.Objects.RelativePotencyFactors {

    [RawTableObjectType(RawDataSourceTableID.RelativePotencyFactors, typeof(RawRelativePotencyFactorRecord))]
    [RawTableObjectType(RawDataSourceTableID.RelativePotencyFactorsUncertain, typeof(RawRelativePotencyFactorUncertainRecord))]
    public sealed class RawRelativePotencyFactorsData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.RelativePotencyFactors;

        public override ActionType ActionType => ActionType.RelativePotencyFactors;

        public List<RawRelativePotencyFactorRecord> RelativePotencyFactors { get; private set; }
        public List<RawRelativePotencyFactorUncertainRecord> RelativePotencyFactorsUncertain { get; private set; }

        public RawRelativePotencyFactorsData() : base() {
            RelativePotencyFactors = new List<RawRelativePotencyFactorRecord>();
            RelativePotencyFactorsUncertain = new List<RawRelativePotencyFactorUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.RelativePotencyFactors, new GenericRawDataTable<RawRelativePotencyFactorRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.RelativePotencyFactors,
                Records = RelativePotencyFactors
            });
            DataTables.Add(RawDataSourceTableID.RelativePotencyFactorsUncertain, new GenericRawDataTable<RawRelativePotencyFactorUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.RelativePotencyFactorsUncertain,
                Records = RelativePotencyFactorsUncertain
            });
        }
    }
}
