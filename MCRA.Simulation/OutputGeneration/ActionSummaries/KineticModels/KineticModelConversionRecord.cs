using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelConversionRecord {

        [Description("Substance from.")]
        [DisplayName("Substance name from")]
        public string SubstanceNameFrom { get; set; }

        [Description("Substance from.")]
        [DisplayName("Substance code from")]
        public string SubstanceCodeFrom { get; set; }

        [Description("Biological matrix from.")]
        [DisplayName("Biological matrix from")]
        public string BiologicalMatrixFrom { get; set; }

        [Description("Dose unit from.")]
        [DisplayName("Dose unit from")]
        public string DoseUnitFrom { get; set; }

        [Description("The expression type or adjustment method of the dose unit (from). This field specifies how the dose unit (source) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type from")]
        public string ExpressionTypeFrom { get; set; }

        [Description("Exposure route from.")]
        [DisplayName("Exposure route from")]
        public string ExposureRouteFrom { get; set; }

        [Description("Substance to.")]
        [DisplayName("Substance name to")]
        public string SubstanceNameTo { get; set; }

        [Description("Substance to.")]
        [DisplayName("Substance code to")]
        public string SubstanceCodeTo { get; set; }

        [Description("Biological matrix to.")]
        [DisplayName("Biological matrix to")]
        public string BiologicalMatrixTo { get; set; }

        [Description("Dose unit to.")]
        [DisplayName("Dose unit to")]
        public string DoseUnitTo { get; set; }

        [Description("The expression type or adjustment method of the dose unit (to). This field specifies how the dose unit (target) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type target")]
        public string ExpressionTypeTo { get; set; }

        [Description("Exposure route to.")]
        [DisplayName("Exposure route target")]
        public string ExposureRouteTo { get; set; }

        [Description("Conversion factor.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }
    }
}
