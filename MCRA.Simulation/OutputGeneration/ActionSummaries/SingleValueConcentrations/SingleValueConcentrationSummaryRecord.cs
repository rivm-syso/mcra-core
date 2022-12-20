using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SingleValueConcentrationSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The concentration value (ConcentrationUnit) derived from the mean of the (positive) concentration values of the concentration data. I.e., after multiplication with substance conversion factors.")]
        [DisplayName("From Mean (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConcentration { get; set; }

        [Description("The concentration value (ConcentrationUnit) derived from the median of the (positive) concentration values. I.e., after multiplication with substance conversion factors.")]
        [DisplayName("From median (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConcentration { get; set; }

        [Description("The concentration value (ConcentrationUnit) derived from the highest residue value of the concentration values. I.e., after multiplication with substance conversion factors.")]
        [DisplayName("From HR (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HighestConcentration { get; set; }

        [Description("The concentration value (ConcentrationUnit) derived from the (highest) LOQ of the concentration data. I.e., after multiplication with substance conversion factors.")]
        [DisplayName("From LOQ (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Loq { get; set; }

        [Description("The concentration value (ConcentrationUnit) derived from the maximum residue limit (MRL). I.e., after multiplication with substance conversion factors.")]
        [DisplayName("From MRL (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mrl { get; set; }

        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

        [Description("The substance conversion factor used to convert the measured substance concentrations to equivalent active substance concentrations.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

    }
}
