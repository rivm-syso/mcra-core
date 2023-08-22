using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelConversionRecord {

        [Description("Substance")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Biological matrix source")]
        [DisplayName("Biological matrix source")]
        public string Source { get; set; }

        [Description("Dose unit from")]
        [DisplayName("Dose unit source")]
        public string DoseUnitFrom { get; set; }

        [Description("The expression type or adjustment method of the dose unit (from). This field specifies how the dose unit (source) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type source")]
        public string ExpressionTypeFrom { get; set; }

        [Description("Exposure route from.")]
        [DisplayName("Exposure route source")]
        public string ExposureRouteFrom { get; set; }

        [Description("Biological matrix target")]
        [DisplayName("Biological matrix target")]
        public string Target { get; set; }

        [Description("Dose unit to")]
        [DisplayName("Dose unit target")]
        public string DoseUnitTo { get; set; }

        [Description("The expression type or adjustment method of the dose unit (to). This field specifies how the dose unit (target) is adjusted, e.g. for blood lipids for fat soluble biomarkers ('mg/g lipids') or the dilution level of the urine ('mg/g creatinine').")]
        [DisplayName("Expression type target")]
        public string ExpressionTypeTo { get; set; }

        [Description("Exposure route to.")]
        [DisplayName("Exposure route target")]
        public string ExposureRouteTo { get; set; }

        [Description("Conversion factor")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }
    }
}
