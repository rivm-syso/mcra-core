using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.AssessmentGroupMembershipModels)]
    public sealed class RawActiveSubstanceModelRecord : IRawDataTableRecord {
        public string id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idEffect { get; set; }
        public double? Accuracy { get; set; }
        public double? Sensitivity { get; set; }
        public double? Specificity { get; set; }
        public string Reference { get; set; }
        public string idIndexSubstance { get; set; }
    }
}
