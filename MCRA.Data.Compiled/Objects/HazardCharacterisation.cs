using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HazardCharacterisation {
        public HazardCharacterisation() {
        }
        private string _name;
        public Compound Substance { get; set; }
        public Effect Effect { get; set; }

        public string Code { get; set; }
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Code;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string Description { get; set; }
        public string PopulationType { get; set; }
        public string TargetOrganString { get; set; }
        public bool IsCriticalEffect { get; set; }
        public string ExposureTypeString { get; set; }
        public string HazardCharacterisationTypeString { get; set; }
        public string Qualifier { get; set; }
        public double Value { get; set; }
        public string DoseUnitString { get; set; }
        public double? CombinedAssessmentFactor { get; set; }
        public string IdPointOfDeparture { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }

        public TargetLevelType TargetLevel { get; set; }

        public ExposureRouteType ExposureRoute { get; set; }

        public ExposureType ExposureType {
            get {
                if (!string.IsNullOrEmpty(ExposureTypeString)) {
                    return ExposureTypeConverter.FromString(ExposureTypeString);
                }
                return ExposureType.Chronic;
            }
        }

        public HazardCharacterisationType HazardCharacterisationType {
            get {
                if (!string.IsNullOrEmpty(HazardCharacterisationTypeString)) {
                    return HazardCharacterisationTypeConverter.FromString(HazardCharacterisationTypeString);
                }
                return HazardCharacterisationType.Unspecified;
            }
        }

        public DoseUnit DoseUnit {
            get {
                if (!string.IsNullOrEmpty(DoseUnitString)) {
                    return DoseUnitConverter.FromString(DoseUnitString);
                }
                return DoseUnit.mgPerKgBWPerDay;
            }
        }

        public BiologicalMatrix TargetOrgan {
            get {
                // NOTE: BiologicalMatrixConverter.FromString cannot handle "Undefined", while this is part of the enum definition.
                if (!string.IsNullOrEmpty(TargetOrganString) && !string.Equals(TargetOrganString, BiologicalMatrix.Undefined.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                    return BiologicalMatrixConverter.FromString(TargetOrganString);
                }
                return BiologicalMatrix.Undefined;
            }
        }
    }
}
