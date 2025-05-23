using System.ComponentModel;
namespace MCRA.Simulation.OutputGeneration {
    public class BurdensOfDiseaseSummaryRecord {

        [Description("Population.")]
        [DisplayName("Population")]
        public string Population { get; set; }

        [Description("Effect.")]
        [DisplayName("Effect")]
        public string Effect { get; set; }

        [Description("Burden of disease indicator.")]
        [DisplayName("BoD indicator")]
        public string BodIndicator { get; set; }

        [Description("The value of the burden of disease indicator.")]
        [DisplayName("Value")]
        public string Value { get; set; }
    }
}

