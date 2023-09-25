using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverCompoundStatisticsRecord {
        [Description("Group individual days (acute) or individuals (chronic) for compound with highest contribution")]
        [DisplayName("Grouped by compound name with highest contribution")]
        public string CompoundName { get; set; }

        [Description("Group individual days (acute) or individuals (chronic) for compound with highest contribution")]
        [DisplayName("Group code")]
        public string CompoundCode { get; set; }

        [Description("Target")]
        [DisplayName("Target")]
        public string Target { get; set; }

        [Description("Median of cumulative exposure (the sum of all exposures on a individual (day))")]
        [DisplayName("Median of cumulative exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double CumulativeExposureMedian { get; set; }

        [Description("Coefficient of variation of cumulative exposure (the sum of all exposures on a individual (day))")]
        [DisplayName("Cv cumulative exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CVCumulativeExposure { get; set; }

        [Description("Median of ratio cumulative exposure/maximum exposure (cumulative exposure is the sum of all exposures on a (individual) day)")]
        [DisplayName("Median of MCR")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RatioMedian { get; set; }

        [Description("Coefficient of variation of MCR")]
        [DisplayName("Cv MCR")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CVRatio { get; set; }

        [Description("Correlation between MCR and cumulative exposure (double logscale)")]
        [DisplayName("Correlation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double R { get; set; }

        [Description("Frequency of individual days (acute) or individuals (chronic) with compound as highest contributor")]
        [DisplayName("Frequency of highest contribution")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Number { get; set; }

       
    }
}

