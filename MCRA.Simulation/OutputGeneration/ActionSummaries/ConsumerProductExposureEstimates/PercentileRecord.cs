using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class PercentileRecord {
        [Display(AutoGenerateField = false)]
        public List<(double Value, double Percentage)> Percentiles { get; set; }

        [Display(Name = "Assessment name", Order = 1)]
        public string AssessmentName { get; set; }

        [Display(Name = "Exposure set", Order = 1)]
        public string ExposureSetCode { get; set; }
        [Display(Name = "Route", Order = 1)]
        public string Route { get; set; }
        [Display(Name = "Substance name", Order = 1)]
        public string SubstanceName { get; set; }
        [Display(Name = "Unit", Order = 1)]
        public string Unit { get; set; }
    }
}
