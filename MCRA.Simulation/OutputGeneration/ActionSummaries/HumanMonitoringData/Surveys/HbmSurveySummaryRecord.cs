using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData.Individuals;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSurveySummaryRecord {

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

        [DisplayName("Number of days per individual")]
        public int NumberOfSurveyDaysPerIndividual { get; set; }

        [DisplayName("Number of individual days")]
        public int NumberOfIndividualDays { get; set; }

        [Display(AutoGenerateField = false)]
        public List<HbmSurveyPropertyRecord> SurveyProperties { get; set; }

    }
}
