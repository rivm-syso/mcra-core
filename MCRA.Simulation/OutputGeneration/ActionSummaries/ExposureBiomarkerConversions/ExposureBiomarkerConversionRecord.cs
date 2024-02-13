using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBiomarkerConversionRecord {

        [Description("Substance name.")]
        [DisplayName("Substance name from")]
        public string SubstanceNameFrom { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code from")]
        public string SubstanceCodeFrom { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type from")]
        public string ExpressionTypeFrom { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit from")]
        public string UnitFrom { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name to")]
        public string SubstanceNameTo { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code to")]
        public string SubstanceCodeTo { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type to")]
        public string ExpressionTypeTo { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit to")]
        public string UnitTo { get; set; }

        [Description("Conversion factor.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Factor { get; set; }

        [Description("Variability (uncertainty) distribution.")]
        [DisplayName("Distribution")]
        public string Distribution { get; set; }

        [Description("Variability upper.")]
        [DisplayName("Variability upper")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double VariabilityUpper { get; set; }

        [Description("Age subgroup information available.")]
        [DisplayName("Age subgroups")]
        public bool IsAgeLower { get; set; }

        [Description("Gender subgroup information available.")]
        [DisplayName("Gender subgroups")]
        public bool IsGender { get; set; }

        [Description("Both age and gender subgroup information available.")]
        [DisplayName("Both present")]
        public bool Both { get; set; }
    }
}
