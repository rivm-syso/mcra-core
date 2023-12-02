using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.HazardCharacterisations)]
    public sealed class RawHazardCharacterisation : IRawDataTableRecord {
        public string idHazardCharacterisation { get; set; }
        public string idEffect { get; set; }
        public string idSubstance { get; set; }
        public string idPopulationType { get; set; }
        public string TargetLevel { get; set; }
        public string ExposureRoute { get; set; }
        public string TargetOrgan { get; set; }
        public string ExpressionType { get; set; }
        public bool IsCriticalEffect { get; set; }
        public string ExposureType { get; set; }
        public string HazardCharacterisationType { get; set; }
        public string Qualifier { get; set; }
        public double Value { get; set; }
        public string DoseUnit { get; set; }
        public string idPointOfDeparture { get; set; }
        public double? CombinedAssessmentFactor { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
