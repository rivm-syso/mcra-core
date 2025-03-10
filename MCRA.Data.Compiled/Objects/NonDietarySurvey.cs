﻿using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietarySurvey {
        private string _name;
        public NonDietarySurvey() {
            NonDietarySurveyProperties = new HashSet<NonDietarySurveyProperty>();
        }

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
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public string IdPopulation { get; set; }

        public ICollection<NonDietarySurveyProperty> NonDietarySurveyProperties { get; set; }

        public ExternalExposureUnit ExposureUnit { get; set; }

        public double ProportionZeros { get; set; }
    }
}
