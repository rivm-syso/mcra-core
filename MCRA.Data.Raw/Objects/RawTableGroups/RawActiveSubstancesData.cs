using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMembershipModels, typeof(RawAssessmentGroupMembershipModel))]
    [RawTableObjectType(RawDataSourceTableID.AssessmentGroupMemberships, typeof(RawAssessmentGroupMembership))]
    public sealed class RawActiveSubstancesData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.AssessmentGroupMemberships;

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public List<RawAssessmentGroupMembershipModel> ActiveSubstanceModels { get; private set; }
        public List<RawAssessmentGroupMembership> ActiveSubstances { get; private set; }

        public RawActiveSubstancesData() : base() {
            ActiveSubstanceModels = [];
            ActiveSubstances = [];
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMembershipModels, new GenericRawDataTable<RawAssessmentGroupMembershipModel>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMembershipModels,
                Records = ActiveSubstanceModels
            });
            DataTables.Add(RawDataSourceTableID.AssessmentGroupMemberships, new GenericRawDataTable<RawAssessmentGroupMembership>() {
                RawDataSourceTableID = RawDataSourceTableID.AssessmentGroupMemberships,
                Records = ActiveSubstances
            });
        }
    }
}
