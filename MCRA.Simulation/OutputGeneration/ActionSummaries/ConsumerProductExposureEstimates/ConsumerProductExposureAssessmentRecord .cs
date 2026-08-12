using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureAssessmentRecord {
        [Description("Assessment name.")]
        [DisplayName("Assessment")]
        public string Name { get; set; }

        [Description("Assessment code.")]
        [DisplayName("Assessment code")]
        public string Code { get; set; }

        [Description("Assessment description.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("Exposure type.")]
        [DisplayName("Exposure type")]
        public string ExposureType { get; set; }

        [Description("Country/Location.")]
        [DisplayName("Country")]
        public string Country { get; set; }

        [Description("Gender.")]
        [DisplayName("Gender")]
        public string Gender { get; set; }

        [Description("Minimum age.")]
        [DisplayName("Minimum age")]
        public double AgeMin { get; set; }

        [Description("Maximum age.")]
        [DisplayName("Maximum age")]
        public double AgeMax { get; set; }

        [Description("Number of exposure sets in assessments")]
        [DisplayName("Number of exposure sets")]
        public int NumberOfSets { get; set; }
    }
}
