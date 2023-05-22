using MCRA.General;

namespace MCRA.Data.Raw.Objects.ActiveSubstance {

    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMembershipModels, typeof(RawActiveSubstanceModelRecord))]
    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMemberships, typeof(RawActiveSubstanceRecord))]
    public sealed class RawActiveSubstancesData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.AssessmentGroupMemberships;

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public List<RawActiveSubstanceModelRecord> ActiveSubstanceModels { get; private set; }
        public List<RawActiveSubstanceRecord> ActiveSubstances { get; private set; }

        public RawActiveSubstancesData() : base() {
            ActiveSubstanceModels = new List<RawActiveSubstanceModelRecord>();
            ActiveSubstances = new List<RawActiveSubstanceRecord>();
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMembershipModels, new GenericRawDataTable<RawActiveSubstanceModelRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMembershipModels,
                Records = ActiveSubstanceModels
            });
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMemberships, new GenericRawDataTable<RawActiveSubstanceRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMemberships,
                Records = ActiveSubstances
            });
        }
    }
}
