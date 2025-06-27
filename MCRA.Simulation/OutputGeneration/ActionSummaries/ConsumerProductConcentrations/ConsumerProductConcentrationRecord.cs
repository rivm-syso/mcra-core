using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductConcentrationRecord {

        [Description("Product name")]
        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [Description("Product code")]
        [DisplayName("Product code")]
        public string ProductCode { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The total number of concentrations.")]
        [DisplayName("Total number of concentrations (N)")]
        public int TotalNumber { get; set; }

        [Description("The number of positive concentrations.")]
        [DisplayName("Positive measurements")]
        public int NumberOfPositives { get; set; }

        [Description("The mean of all concentrations.")]
        [DisplayName("Mean all concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("The median of all concentrations.")]
        [DisplayName("Median all concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("The lower percentile point of all concentrations.")]
        [DisplayName("Lower {LowerPercentage} all concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentile { get; set; }

        [Description("The upper percentile point of all concentrations.")]
        [DisplayName("Upper {UpperPercentage} all concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentile { get; set; }

        [Description("The mean of the positive concentrations.")]
        [DisplayName("Mean positive concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("The median of the positive concentrations.")]
        [DisplayName("Median positive concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("The lower percentile point of the positive concentrations.")]
        [DisplayName("Lower {LowerPercentage} positive concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("The upper percentile point of the positive concentrations.")]
        [DisplayName("Upper {UpperPercentage} positive concentrations (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }
    }
}
