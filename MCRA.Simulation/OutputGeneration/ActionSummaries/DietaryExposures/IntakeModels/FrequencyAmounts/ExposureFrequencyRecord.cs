using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureFrequencyRecord {
        [Description("Observations or individuals.")]
        [DisplayName("")]
        public string Description { get; set; }

        [Description("Observations or individuals.")]
        [DisplayName("All exposures (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfExposures{ get; set; }

        [Description("Observations or individuals.")]
        [DisplayName("Positive exposures (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfPositives{ get; set; }

        [Description("Observations or individuals.")]
        [DisplayName("Percentage positive exposures (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageOfPositives { get; set; }
    }
}
