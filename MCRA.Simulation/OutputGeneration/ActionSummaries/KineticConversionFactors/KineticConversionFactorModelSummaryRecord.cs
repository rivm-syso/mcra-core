using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticConversionFactorModelSummaryRecord {

        [Description("Substance name from.")]
        [DisplayName("Substance name from")]
        public string SubstanceNameFrom { get; set; }

        [Description("Substance code from.")]
        [DisplayName("Substance code from")]
        public string SubstanceCodeFrom { get; set; }

        [Description("Exposure route from.")]
        [DisplayName("Exposure route from")]
        public string ExposureRouteFrom { get; set; }

        [Description("Biological matrix from.")]
        [DisplayName("Biological matrix from")]
        public string BiologicalMatrixFrom { get; set; }

        [Description("Unit from.")]
        [DisplayName("Unit from")]
        public string UnitFrom { get; set; }

        [Description("The expression type or adjustment method of the dose unit (from). This field specifies how the dose unit (source) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type from")]
        public string ExpressionTypeFrom { get; set; }

        [Description("Substance name to.")]
        [DisplayName("Substance name to")]
        public string SubstanceNameTo { get; set; }

        [Description("Substance code to.")]
        [DisplayName("Substance code to")]
        public string SubstanceCodeTo { get; set; }

        [Description("Exposure route to.")]
        [DisplayName("Exposure route to")]
        public string ExposureRouteTo { get; set; }

        [Description("Biological matrix to.")]
        [DisplayName("Biological matrix to")]
        public string BiologicalMatrixTo { get; set; }

        [Description("Unit to.")]
        [DisplayName("Unit to")]
        public string UnitTo { get; set; }

        [Description("The expression type or adjustment method of the dose unit (to). This field specifies how the dose unit (target) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type target")]
        public string ExpressionTypeTo { get; set; }

        [Description("Conversion factor.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NominalFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> UncertaintyValues { get; set; }

        [Description("Conversion factor uncertainty median bound (p50).")]
        [DisplayName("Unc median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UncertaintyMedian {
            get {
                if (UncertaintyValues?.Count > 0) {
                    return UncertaintyValues.Percentile(50);
                }
                return double.NaN;
            }
        }

        [Description("Conversion factor uncertainty lower bound (p2.5).")]
        [DisplayName("Unc lower (p2.5)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UncertaintyLowerBoundPercentile {
            get {
                if (UncertaintyValues?.Count > 0) {
                    return UncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Conversion factor uncertainty upper bound (p97.5).")]
        [DisplayName("Unc upper (p97.5)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UncertaintyUpperBoundPercentile {
            get {
                if (UncertaintyValues?.Count > 0) {
                    return UncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("Age subgroup information available.")]
        [DisplayName("Age subgroups")]
        public bool HasCovariateAge { get; set; }

        [Description("Sex subgroup information available.")]
        [DisplayName("Sex subgroups")]
        public bool HasCovariateSex { get; set; }

        public string GetKey() {
            var keys = new string[] {
                SubstanceCodeFrom,
                ExposureRouteFrom,
                BiologicalMatrixFrom,
                UnitFrom,
                ExpressionTypeFrom,
                SubstanceCodeTo,
                ExposureRouteTo,
                BiologicalMatrixTo,
                UnitTo,
                ExpressionTypeTo
            };
            return string.Join('_', keys.Where(r => !string.IsNullOrEmpty(r)));
        }

        public bool IsExternalTargetFrom() {
            return !string.IsNullOrEmpty(ExposureRouteFrom);
        }

        public bool IsExternalTargetTo() {
            return !string.IsNullOrEmpty(ExposureRouteTo);
        }

        public string GetTargetNameTo() {
            if (!string.IsNullOrEmpty(ExposureRouteTo)) {
                return ExposureRouteTo;
            } else {
                return BiologicalMatrixTo;
            }
        }

        public string GetTargetNameFrom() {
            if (!string.IsNullOrEmpty(ExposureRouteFrom)) {
                return ExposureRouteFrom;
            } else {
                return BiologicalMatrixFrom;
            }
        }
    }
}
