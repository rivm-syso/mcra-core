using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductExposuresDataRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Mean consumer product exposure (ConsumerProductExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> ConsumerProductUncertaintyValues { get; set; }

        [Description("uncertainty lower bound (2.5 percentile).")]
        [DisplayName("uncertainty lower bound (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConsumerProductLowerBoundPercentile {
            get {
                if (ConsumerProductUncertaintyValues.Count > 1) {
                    return ConsumerProductUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty upper bound (97.5 percentile).")]
        [DisplayName("uncertainty upper bound (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConsumerProductUpperBoundPercentile {
            get {
                if (ConsumerProductUncertaintyValues.Count > 1) {
                    return ConsumerProductUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty mean value.")]
        [DisplayName("uncertainty mean value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConsumerProductMeanUncertaintyValue {
            get {
                if (ConsumerProductUncertaintyValues.Count > 1) {
                    return ConsumerProductUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }
    }
}