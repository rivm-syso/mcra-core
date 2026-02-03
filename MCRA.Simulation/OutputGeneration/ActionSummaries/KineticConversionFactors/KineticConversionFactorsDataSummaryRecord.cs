using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticConversionFactorsDataSummaryRecord {

        [Description("Kinetic conversion factor model code.")]
        [DisplayName("Model code")]
        public string KcfModelCode { get; set; }

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

        [Description("Dose unit from.")]
        [DisplayName("Dose unit from")]
        public string DoseUnitFrom { get; set; }

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

        [Description("Dose unit to.")]
        [DisplayName("Dose unit to")]
        public string DoseUnitTo { get; set; }

        [Description("The expression type or adjustment method of the dose unit (to). This field specifies how the dose unit (target) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type target")]
        public string ExpressionTypeTo { get; set; }

        [Description("Conversion factor.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

        [Description("Uncertainty distribution type (uniform or lognormal.")]
        [DisplayName("Distribution type")]
        public string DistributionType { get; set; }

        [Description("Uncertainty upper.")]
        [DisplayName("Uncertainty upper")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UncertaintyUpper { get; set; }

        [Description("Age subgroup information available.")]
        [DisplayName("Age subgroups")]
        public bool HasCovariateAge { get; set; }

        [Description("Gender subgroup information available.")]
        [DisplayName("Gender subgroups")]
        public bool HasCovariateSex { get; set; }

    }
}
