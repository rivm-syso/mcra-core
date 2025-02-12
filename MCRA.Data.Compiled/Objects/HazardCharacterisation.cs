﻿using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HazardCharacterisation {

        public HazardCharacterisation() { }

        private string _name;
        public Compound Substance { get; set; }
        public Effect Effect { get; set; }

        public string Code { get; set; }
        public string Name {
            get => string.IsNullOrEmpty(_name) ? Code : _name;
            set => _name = value;
        }
        public string Description { get; set; }
        public string PopulationType { get; set; }
        public ExpressionType ExpressionType { get; set; }
        public BiologicalMatrix BiologicalMatrix { get; set; }
        public bool IsCriticalEffect { get; set; }
        public string Qualifier { get; set; }
        public double Value { get; set; }
        public double? CombinedAssessmentFactor { get; set; }
        public string IdPointOfDeparture { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }

        public TargetLevelType TargetLevel { get; set; }

        public ExposureRoute ExposureRoute { get; set; }

        public ICollection<HazardCharacterisationUncertain> HazardCharacterisationsUncertains { get; set; } = [];

        public ICollection<HCSubgroup> HCSubgroups { get; set; } = [];

        public ExposureType ExposureType { get; set; }

        public HazardCharacterisationType HazardCharacterisationType { get; set; }

        public DoseUnit DoseUnit { get; set; }
        public ExposureTarget ExposureTarget {
            get {
                return TargetLevel == TargetLevelType.External
                    ? new ExposureTarget(ExposureRoute)
                    : new ExposureTarget(BiologicalMatrix, ExpressionType);
            }
        }
    }
}
