using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseFitRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Covariate level")]
        public string CovariateLevel { get; set; }

        //[DisplayName("BMR (abs)")]
        //[DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double BenchmarkResponse { get; set; }

        [DisplayName("BMD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDose { get; set; }

        [DisplayName("BMDL")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseLower { get; set; }

        [DisplayName("BMDU")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseUpper { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> BenchmarkDosesUncertain { get; set; }

        [DisplayName("BMD bootstrap (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainMedian {
            get {
                if (BenchmarkDosesUncertain?.Any() ?? false) {
                    return BenchmarkDosesUncertain.Median();
                }
                return double.NaN;
            }
        }

        [DisplayName("BMD bootstrap (P5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainLowerBoundPercentile {
            get {
                if (BenchmarkDosesUncertain?.Any() ?? false) {
                    return BenchmarkDosesUncertain.Percentile(5);
                }
                return double.NaN;
            }
        }

        [DisplayName("BMD bootstrap (P95)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainUpperBoundPercentile {
            get {
                if (BenchmarkDosesUncertain?.Any() ?? false) {
                    return BenchmarkDosesUncertain.Percentile(95);
                }
                return double.NaN;
            }
        }

        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativePotencyFactor { get; set; }

        [DisplayName("RPFL")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? RpfLower { get; set; }

        [DisplayName("RPFU")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? RpfUpper { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> RpfUncertain { get; set; }

        [DisplayName("RPF bootstrap (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainMedian {
            get {
                if (RpfUncertain?.Any() ?? false) {
                    return RpfUncertain.Median();
                }
                return double.NaN;
            }
        }

        [DisplayName("RPF bootstrap (P5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainLowerBoundPercentile {
            get {
                if (RpfUncertain?.Any() ?? false) {
                    return RpfUncertain.Percentile(5);
                }
                return double.NaN;
            }
        }

        [DisplayName("RPF bootstrap (P95)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainUpperBoundPercentile {
            get {
                if (RpfUncertain?.Any() ?? false) {
                    return RpfUncertain.Percentile(95);
                }
                return double.NaN;
            }
        }

        [DisplayName("Model parameters")]
        public string ModelParameterValues { get; set; }

    }
}
