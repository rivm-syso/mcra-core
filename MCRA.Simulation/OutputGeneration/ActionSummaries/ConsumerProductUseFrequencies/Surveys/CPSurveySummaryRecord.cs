using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CPSurveySummaryRecord {

        [DisplayName("Survey name")]
        public string Name { get; set; }

        [DisplayName("Survey code")]
        public string Code { get; set; }

        [DisplayName("Sampling start date")]
        public string StartDate { get; set; }

        [DisplayName("Sampling end date")]
        public string EndDate { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Number of individuals in survey")]
        public int NumberOfIndividuals { get; set; }

        [Display(AutoGenerateField = false)]
        public List<SurveyPropertyRecord> SurveyProperties { get; set; }

    }
}
