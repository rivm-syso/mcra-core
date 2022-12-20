using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.QsarMembershipScores)]
    public class RawQsarMembershipScore {
        public string idQSARMembershipModel { get; set; }
        public string idSubstance { get; set; }
        public double MembershipScore { get; set; }
    }
}
