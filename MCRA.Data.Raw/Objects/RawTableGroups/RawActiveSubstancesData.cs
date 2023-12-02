using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMembershipModels, typeof(RawActiveSubstanceModel))]
    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMemberships, typeof(RawActiveSubstance))]
    public sealed class RawActiveSubstancesData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.AssessmentGroupMemberships;

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public List<RawActiveSubstanceModel> ActiveSubstanceModels { get; private set; }
        public List<RawActiveSubstance> ActiveSubstances { get; private set; }

        public RawActiveSubstancesData() : base() {
            ActiveSubstanceModels = new List<RawActiveSubstanceModel>();
            ActiveSubstances = new List<RawActiveSubstance>();
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMembershipModels, new GenericRawDataTable<RawActiveSubstanceModel>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMembershipModels,
                Records = ActiveSubstanceModels
            });
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMemberships, new GenericRawDataTable<RawActiveSubstance>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMemberships,
                Records = ActiveSubstances
            });
        }
    }
}
