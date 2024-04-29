using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PBKDrilldownRecord {
        [Description("Individual id.")]
        [DisplayName("Individual id")]
        public string IndividualCode { get; set; }

        [DisplayName("BodyWeight ({BodyWeightUnit})")]
        public double BodyWeight { get; set; }

        [Description("Compartment.")]
        [DisplayName("Compartment")]
        public string Compartment { get; set; }

        [Description("Compartment weight or volume.")]
        [DisplayName("Compartment weight")]
        public double CompartmentWeight { get; set; }

        [DisplayName("Relative compartment weight")]
        public double RelativeCompartmentWeight { get; set; }

        [DisplayName("External daily substance exposure amount ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExternalDailyExposureAmount { get; set; }

        [DisplayName("Dietary ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Dietary { get; set; }

        [DisplayName("Oral ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Oral { get; set; }

        [DisplayName("Dermal ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Dermal { get; set; }

        [DisplayName("Inhalation ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Inhalation { get; set; }

        [DisplayName("External exposure ({ExternalExposureUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExternalExposure { get { return ExternalDailyExposureAmount / BodyWeight; } }

        [Description("Acute exposure.")]
        [DisplayName("Peak internal substance amount ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PeakSubstanceAmount { get; set; }

        [Description("Acute exposure.")]
        [DisplayName("Peak internal exposure ({TargetConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PeakExposureAmount { get; set; }

        [Description("Acute exposure.")]
        [DisplayName("Ratio peak internal / external")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RatioPeak { get; set; }

        [Description("Chronic exposure.")]
        [DisplayName("Long term internal substance amount ({TargetAmountUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LongSubstanceAmount { get; set; }

        [Description("Chronic exposure.")]
        [DisplayName("Long term internal exposure ({TargetConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LongExposureAmount { get; set; }

        [Description("Chronic exposure.")]
        [DisplayName("Ratio long term internal / external")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RatioLong { get; set; }
    }
}
