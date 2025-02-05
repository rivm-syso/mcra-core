using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Overall individual drilldown.
    /// </summary>
    public sealed class IndividualSubstanceDrillDownRecord  {

        [Description("Day in survey.")]
        [DisplayName("Day")]
        public string Day { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Per substance: exposure per person day.")]
        [DisplayName("Exposure per person day (PerPersonIntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExposurePerDay { get; set; }

        [Description("Per substance: exposure  (= exposure per person/ body weight).")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Relative Potency Factor.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Rpf { get; set; }

        [Description("Equivalent exposure.")]
        [DisplayName("Equivalent exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double EquivalentExposure { get; set; }
    }
}
