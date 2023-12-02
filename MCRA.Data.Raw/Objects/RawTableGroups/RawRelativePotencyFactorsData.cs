using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.RelativePotencyFactors, typeof(RawRelativePotencyFactor))]
    [RawTableObjectType(RawDataSourceTableID.RelativePotencyFactorsUncertain, typeof(RawRelativePotencyFactorUncertain))]
    public sealed class RawRelativePotencyFactorsData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.RelativePotencyFactors;

        public override ActionType ActionType => ActionType.RelativePotencyFactors;

        public List<RawRelativePotencyFactor> RelativePotencyFactors { get; private set; }
        public List<RawRelativePotencyFactorUncertain> RelativePotencyFactorsUncertain { get; private set; }

        public RawRelativePotencyFactorsData() : base() {
            RelativePotencyFactors = new List<RawRelativePotencyFactor>();
            RelativePotencyFactorsUncertain = new List<RawRelativePotencyFactorUncertain>();
            DataTables.Add(RawDataSourceTableID.RelativePotencyFactors, new GenericRawDataTable<RawRelativePotencyFactor>() {
                RawDataSourceTableID = RawDataSourceTableID.RelativePotencyFactors,
                Records = RelativePotencyFactors
            });
            DataTables.Add(RawDataSourceTableID.RelativePotencyFactorsUncertain, new GenericRawDataTable<RawRelativePotencyFactorUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.RelativePotencyFactorsUncertain,
                Records = RelativePotencyFactorsUncertain
            });
        }
    }
}
