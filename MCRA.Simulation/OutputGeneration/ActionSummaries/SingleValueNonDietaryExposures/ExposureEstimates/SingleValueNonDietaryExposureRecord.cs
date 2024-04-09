using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class SingleValueNonDietaryExposureRecord {

        [Display(Name = "Scenario", Order = 1)]
        public string ScenarioName { get; set; }

        [Display(Name = "Substance", Order = 2)]
        public string SubstanceName { get; set; }

        [Display(Name = "Substance code", Order = 3)]
        public string SubstanceCode { get; set; }

        [Display(Name = "Exposure source", Order = 4)]
        public string ExposureSource { get; set; }

        [Display(Name = "Exposure determinants ID", Order = 5)]
        public string ExposureDeterminantCombinationId { get; set; }

        [Display(Name = "Exposure determinants name", Order = 6)]
        public string ExposureDeterminantCombinationName { get; set; }

        [Description("Exposure (ExposureUnit).")]
        [Display(Name = "Value (ExposureUnit)", Order = 7)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExposureValue { get; set; }

        [Display(Name = "Estimate type", Order = 8)]
        public string EstimateType { get; set; }
    }
}

