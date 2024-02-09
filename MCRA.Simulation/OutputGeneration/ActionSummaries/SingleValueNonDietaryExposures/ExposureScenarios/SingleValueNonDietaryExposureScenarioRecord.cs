using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class SingleValueNonDietaryExposureScenarioRecord {
        [Display(Name = "Scenario", Order = 1)]
        public string ScenarioName { get; set; }

        [Display(Name = "Description", Order = 2)]
        public string Description { get; set; }

        [Display(Name = "Population", Order = 3)]
        public string Population { get; set; }

        [Display(Name = "Exposure type", Order = 4)]
        public string ExposureType { get; set; }

        [Display(Name = "Exposure level", Order = 5)]
        public string ExposureLevel { get; set; }

        [Display(Name = "Exposure routes", Order = 6)]
        public string ExposureRoutes { get; set; }
    }
}
