using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class NediSingleValueDietaryExposuresRecord : ChronicSingleValueDietaryExposureRecord {

        [Description("Large portion (ConsumptionUnit).")]
        [Display(Name = "Large portion (ConsumptionUnit)", Order = 11)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LargePortion { get; set; }

        [Description("High exposure estimate for the population.")]
        [Display(Name = "High exposure (ExposureUnit)", Order = 31)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HighExposure { get; set; }

    }
}

