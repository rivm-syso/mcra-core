using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for kinetic conversion factors.
    /// </summary>
    public sealed class KineticConversionFactorRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Exposure route")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("Kinetic conversion factor.")]
        [DisplayName("Kinetic conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double KineticConversionFactor { get; set; }

        [Description("Mean kinetic conversion factor.")]
        [DisplayName("Mean kinetic conversion factor")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanKineticConversionFactor { get { return KineticConversionFactors.Any() ? KineticConversionFactors.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> KineticConversionFactors { get; set; }

        [Description("Lower uncertainty bound kinetic conversion factor.")]
        [DisplayName("Lower bound (%) (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerKineticConversionFactor { get { return KineticConversionFactors.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound kinetic conversion factor.")]
        [DisplayName("Upper bound (%) (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperKineticConversionFactor { get { return KineticConversionFactors.Percentile(UncertaintyUpperBound); } }
    }
}
