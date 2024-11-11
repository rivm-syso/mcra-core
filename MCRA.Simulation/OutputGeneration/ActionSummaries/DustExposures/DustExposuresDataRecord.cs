using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DustExposuresDataRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Mean dust exposure (DustExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> DustUncertaintyValues { get; set; }

        [Description("uncertainty lower bound (2.5 percentile).")]
        [DisplayName("uncertainty lower bound (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustLowerBoundPercentile {
            get {
                if (DustUncertaintyValues.Count > 1) {
                    return DustUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty upper bound (97.5 percentile).")]
        [DisplayName("uncertainty upper bound (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustUpperBoundPercentile {
            get {
                if (DustUncertaintyValues.Count > 1) {
                    return DustUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty mean value.")]
        [DisplayName("uncertainty mean value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustMeanUncertaintyValue {
            get {
                if (DustUncertaintyValues.Count > 1) {
                    return DustUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }
    }
}