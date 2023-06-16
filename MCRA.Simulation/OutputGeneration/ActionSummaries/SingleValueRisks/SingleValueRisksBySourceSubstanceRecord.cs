using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksBySourceSubstanceRecord {

        [Description("The route of the exposure.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Source name")]
        public string SourceName { get; set; }

        [DisplayName("Source code")]
        public string SourceCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Estimate of the substance exposure through the specified route/source (in {SingleValueExposuresUnit}).")]
        [Display(Name = "Exposure (SingleValueExposuresUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExposureValue { get; set; }

        [Description("The threshold dose (in {HazardCharacterisationsUnit}) for the substance considered for the selected health effect.")]
        [Display(Name = "Hazard characterisation (HazardCharacterisationsUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardCharacterisationValue { get; set; }

        [Description("The type of the point-of-departure from which the hazard characterisation was derived. Note that the hazard characterisation may also include additional assessment factors (e.g., a safety factor).")]
        [DisplayName("Hazard characterisation origin")]
        public string PotencyOrigin { get; set; }

        [Description("Percentage of the exposure relative to the reference dose (i.e., the hazard characterisation) computed as %RfD = EXP / HC * 100%.")]
        [Display(Name = "Percentage of reference dose")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentageOfReferenceDose {
            get {
                return ExposureThresholdRatio * 100D;
            }
        }

        [Description("Computed as exposure divided by the hazard characterisation (Exp/Threshold).")]
        [Display(Name = "Exposure/threshold value ratio")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExposureThresholdRatio { get; set; }

        [Description("Computed as the hazard characterisation divided by the exposure (Threshold/Exp).")]
        [Display(Name = "Threshold value/exposure ratio")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ThresholdExposureRatio { get; set; }
    }
}
