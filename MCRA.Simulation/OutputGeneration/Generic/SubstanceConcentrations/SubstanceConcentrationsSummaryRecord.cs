using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SubstanceConcentrationsSummaryRecord {
        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The total number of samples.")]
        [DisplayName("Total samples (N)")]
        public int SamplesTotal { get; set; }

        [Description("The number of positive measurement values.")]
        [DisplayName("Positives")]
        public int PositiveMeasurements { get; set; }

        [Description("The mean of the positive measurement values.")]
        [DisplayName("Mean positive samples")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("The median of the positive measurement values.")]
        [DisplayName("Median positive samples")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("The lower percentile point of the positive measurement values.")]
        [DisplayName("Lower {LowerPercentage} positives")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("The upper percentile point of the positive measurement values.")]
        [DisplayName("Upper {UpperPercentage} positives")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }
    }
}
