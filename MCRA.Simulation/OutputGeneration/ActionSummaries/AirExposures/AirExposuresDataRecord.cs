using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class AirExposuresDataRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Mean air exposure (AirExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AirUncertaintyValues { get; set; }

        [Description("uncertainty lower bound (2.5 percentile).")]
        [DisplayName("uncertainty lower bound (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AirLowerBoundPercentile {
            get {
                if (AirUncertaintyValues.Count > 1) {
                    return AirUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty upper bound (97.5 percentile).")]
        [DisplayName("uncertainty upper bound (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AirUpperBoundPercentile {
            get {
                if (AirUncertaintyValues.Count > 1) {
                    return AirUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty mean value.")]
        [DisplayName("uncertainty mean value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AirMeanUncertaintyValue {
            get {
                if (AirUncertaintyValues.Count > 1) {
                    return AirUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }
    }
}