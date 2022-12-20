using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.QsarMembershipModels)]
    public class RawQsarMembershipModel {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string idEffect { get; set; }
        public double Accuracy { get; set; }
        public double Sensitivity { get; set; }
        public double Specificity { get; set; }
        public string Reference { get; set; }
    }
}
