using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelTimeCourseDrilldownRecord {

        [Description("Individual id.")]
        [DisplayName("Individual id")]
        public string IndividualCode { get; set; }

        [Description("Reference body weight of the simulated individual.")]
        [DisplayName("BodyWeight ({BodyWeightUnit})")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double BodyWeight { get; set; }

        [Description("Reference age of the simulated individual.")]
        [DisplayName("Age ({AgeUnit})")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double Age { get; set; }

        [DisplayName("External exposure ({ExternalExposureUnit})")]
        [Description("Total external exposure ({ExternalExposureUnit}).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExternalExposure { get; set; }

        [DisplayName("Oral ({ExternalExposureUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Oral { get; set; }

        [DisplayName("Dermal ({ExternalExposureUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Dermal { get; set; }

        [DisplayName("Inhalation ({ExternalExposureUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Inhalation { get; set; }

        [Description("Internal exposure.")]
        [DisplayName("Internal exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TargetExposure { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("The unit of the internal exposure.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The way in which the concentration values are standardised, normalised, or otherwise expressed.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Description("Compartment size (mass or volume).")]
        [DisplayName("Compartment size")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CompartmentWeight {
            get {
                return BodyWeight * RelativeCompartmentWeight;
            }
        }

        [Description("Ratio internal / external exposure.")]
        [DisplayName("Ratio internal / external")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RatioInternalExternal { get; set; }

        [Display(AutoGenerateField = false)]
        [Description("Relative compartment Weight.")]
        [DisplayName("Relative compartment Weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativeCompartmentWeight { get; set; }

        [Description("The absolute maximum of the of the internal exposures time course.")]
        [DisplayName("The absolute maximum of the of the internal exposures time course")]
        [Display(AutoGenerateField = false)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MaximumTargetExposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<TargetIndividualExposurePerTimeUnitRecord> TargetExposures { get; set; }
    }
}
