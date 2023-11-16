using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverSubstanceRecord {
        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName  { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Exposure target")]
        [DisplayName("Exposure target")]
        public string Target { get; set; }

        [Description("Cumulative exposure")]
        [DisplayName("Cumulative exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double CumulativeExposure { get; set; }

        [Description("Ratio cumulative exposure/maximum exposure")]
        [DisplayName("Ratio cumulative exposure/maximum exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Ratio { get; set; }
    }
}
