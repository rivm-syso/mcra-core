using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryExposuresSummaryRecord {

        [DisplayName("Non-dietary survey")]
        public string NonDietarySurvey { get; set; }
        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        [Description("The exposure unit of the exposure values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [DisplayName("Total individuals")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividuals { get; set; }

        [DisplayName("Individuals with positive exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositiveIndividuals { get; set; }

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
