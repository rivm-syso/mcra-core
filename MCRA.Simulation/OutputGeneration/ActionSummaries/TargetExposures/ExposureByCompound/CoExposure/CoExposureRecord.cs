using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public class CoExposureRecord {
        [Description("Number of substances occurring together")]
        [Display(Name = "Number of substances", Order = 1)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSubstances { get; set; }

        [Description("Frequency")]
        [Display(Name="Frequency", Order = 2)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Frequency { get; set; }

        [Description("Percentage of total")]
        [Display(Name="Percentage", Order = 3)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

    }
}
