using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductExposureAssessment {
        private string _name;
        public string IdAssessment { get; set; }

        public string Name {
            get => string.IsNullOrEmpty(_name) ? IdAssessment : _name;
            set => _name = value;
        }
        public string Description { get; set; }
        public ExposureType ExposureType { get; set; }
        public string Location { get; set; }
        public double? AgeMin { get; set; }
        public double? AgeMax { get; set; }
        public GenderType Gender { get; set; }
        public string Reference { get; set; }
        public List<ConsumerProductExposureSet> ExposureSets { get; set; }
    }
}
