using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseFitRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Covariate level")]
        public string CovariateLevel { get; set; }

        [Display(AutoGenerateField = false)]
        public double BenchmarkResponse { get; set; }

        [Description("Benchmark dose.")]
        [DisplayName("BMD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDose { get; set; }

        [Description("Benchmark dose lower confidence limit (p5) based on fitted model.")]
        [DisplayName("BMD confidence limit p5")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseLowerConfidenceLimit { get { return BenchmarkDoseLower; } }

        [Description("Benchmark dose upper confidence limit (p95) based on fitted model.")]
        [DisplayName("BMD confidence limit p95")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseUpperConfidenceLimit { get { return BenchmarkDoseUpper; } }

        [Description("Benchmark dose lower limit (p5) based on bootstrap.")]
        [DisplayName("BMD lower")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseLower { get; set; }

        [Description("Benchmark dose upper limit (p95) based on bootstrap.")]
        [DisplayName("BMD upper")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? BenchmarkDoseUpper { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> BenchmarkDosesUncertain { get; set; }

        [Description("Benchmark dose median (p50) based on bootstrap.")]
        [DisplayName("BMD (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainMedian {
            get {
                if (BenchmarkDosesUncertain?.Count > 0) {
                    return BenchmarkDosesUncertain.Median();
                }
                return double.NaN;
            }
        }

        [Description("Benchmark dose lower limit (p5) based on bootstrap.")]
        [DisplayName("BMD (p5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainLowerBoundPercentile {
            get {
                if (BenchmarkDosesUncertain?.Count > 0) {
                    return BenchmarkDosesUncertain.Percentile(5);
                }
                return double.NaN;
            }
        }

        [Description("Benchmark dose upper limit (p95) based on bootstrap.")]
        [DisplayName("BMD (p95)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDosesUncertainUpperBoundPercentile {
            get {
                if (BenchmarkDosesUncertain?.Count > 0) {
                    return BenchmarkDosesUncertain.Percentile(95);
                }
                return double.NaN;
            }
        }

        [Description("Relative potency factor.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativePotencyFactor { get; set; }

        [Description("Relative potency factor lower limit (p5).")]
        [DisplayName("RPFL")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? RpfLower { get; set; }

        [Description("Relative potency factor upper limit (p95).")]
        [DisplayName("RPFU")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? RpfUpper { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> RpfUncertain { get; set; }

        [Description("Relative potency factor median (p50).")]
        [DisplayName("RPF bootstrap (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainMedian {
            get {
                if (RpfUncertain?.Count > 0) {
                    return RpfUncertain.Median();
                }
                return double.NaN;
            }
        }

        [Description("Relative potency factor lower limit (p95).")]
        [DisplayName("RPF bootstrap (Pp5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainLowerBoundPercentile {
            get {
                if (RpfUncertain?.Count > 0) {
                    return RpfUncertain.Percentile(5);
                }
                return double.NaN;
            }
        }

        [Description("Relative potency factor upper limit (p95).")]
        [DisplayName("RPF bootstrap (Pp95)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RpfUncertainUpperBoundPercentile {
            get {
                if (RpfUncertain?.Count > 0) {
                    return RpfUncertain.Percentile(95);
                }
                return double.NaN;
            }
        }
        [Description("Model parameters")]
        [DisplayName("Model parameters")]
        public string ModelParameterValues { get; set; }

    }
}
