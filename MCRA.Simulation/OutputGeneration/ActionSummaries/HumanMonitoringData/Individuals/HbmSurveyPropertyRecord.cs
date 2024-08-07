using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData.Individuals {

    public sealed class HbmSurveyPropertyRecord {

        [DisplayName("Code")]
        public string Code { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Covariate")]
        public string CovariateName { get; set; }

        [DisplayName("PropertyType")]
        public string PropertyType { get; set; }

        [DisplayName("Level")]
        public string Level { get; set; }

        [DisplayName("Minimum value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Minimum { get; set; }

        [DisplayName("Maximum value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Maximum { get; set; }

    }
}