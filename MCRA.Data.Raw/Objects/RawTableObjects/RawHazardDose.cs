using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.HazardDoses)]
    public sealed class RawHazardDose : IRawDataTableRecord {
        public string idDoseResponseModel { get; set; }
        public string idEffect { get; set; }
        public string idCompound { get; set; }
        public string Species { get; set; }
        public string ModelCode { get; set; }
        public string DoseResponseModelEquation { get; set; }
        public string DoseResponseModelParameterValues { get; set; }
        public double LimitDose { get; set; }
        public string HazardDoseType { get; set; }
        public string DoseUnit { get; set; }
        public string CriticalEffectSize { get; set; }
        public string ExposureRoute { get; set; }
        public bool IsCriticalEffect { get; set; }
        public string TargetLevel { get; set; }
        public string BiologicalMatrix { get; set; }
        public string ExpressionType { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }
    }
}
