using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TestSystemsSummaryRecord {

        [DisplayName("Code")]
        public string CodeSystem { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Test system")]
        public string TestSystemType { get; set; }

        [DisplayName("Organ")]
        public string Organ { get; set; }

        [DisplayName("Species")]
        public string Species { get; set; }

        [DisplayName("Strain")]
        public string Strain { get; set; }

        [DisplayName("Exposure route")]
        public string ExposureRouteType { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Model for target")]
        public bool ModelForTarget { get; set; }

        [DisplayName("Guideline")]
        public string GuidelineStudy { get; set; }

        [DisplayName("Reference")]
        public string Reference { get; set; }
    }
}
