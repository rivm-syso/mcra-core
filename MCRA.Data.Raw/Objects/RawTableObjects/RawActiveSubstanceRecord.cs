using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.AssessmentGroupMemberships)]
    public sealed class RawActiveSubstanceRecord : IRawDataTableRecord {
        public string idGroupMembershipModel { get; set; }
        public string idCompound { get; set; }
        public double MembershipProbability { get; set; }
    }
}
