using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class AttributableEbdSummaryRecord {

        [Description("Percentile interval.")]
        [DisplayName("Percentile interval")]
        public string PercentileInterval { get; set; }

        [Description("Exposure level for percentile.")]
        [DisplayName("Exposure level")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExposureLevel { get; set; }

        [Description("The target unit of the exposure level.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Percentile specific Odds Ratio.")]
        [DisplayName("Percentile specific OR")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentileSpecificOr { get; set; }

        [Description("Percentile specific Attributable Fraction.")]
        [DisplayName("Percentile specific AF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentileSpecificAf { get; set; }

        [Description("Absolute Burden of Disease.")]
        [DisplayName("Absolute Burden of Disease")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double AbsoluteBod { get; set; }

        [Description("Burden of disease attributable to part of population identified by percentage.")]
        [DisplayName("Attributable Burden of Disease")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double AttributableEbd { get; set; }
    }
}
