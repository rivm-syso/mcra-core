using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MCRDrilldownRecord {
        [Description("Tail percentage, exposures above this percentile are analysed.")]
        [DisplayName("Tail %")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Tail { get; set; }

        [Description("% of tail exposure due to samples with single substances (MCR=1).")]
        [DisplayName("% with MCR=1")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage1 { get; set; }

        [Description("List of substances, that occur as only substance in a tail sample.")]
        [DisplayName("Substances")]
        public string Substances1 { get; set; }

        [Description("% of tail exposure due to samples with multiple substances, but MCR <= 2.")]
        [DisplayName("% with 1<MCR<=2")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage2 { get; set; }

        [Description("List of substances, that occur as only substance in a tail sample.")]
        [DisplayName("Substances")]
        public string Substances2 { get; set; }

        [Description("% of tail exposure due to samples with multiple substances, with MCR > 2.")]
        [DisplayName("% with MCR>2")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage3 { get; set; }

        [Description("List of substances, that occur as only substance in a tail sample.")]
        [DisplayName("Substances")]
        public string Substances3 { get; set; }

        [Description("Number of individual days (acute) or individuals (chronic).")]
        [DisplayName("No {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Number { get; set; }

    }
}

