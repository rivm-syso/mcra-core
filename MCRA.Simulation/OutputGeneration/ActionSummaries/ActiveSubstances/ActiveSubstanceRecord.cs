using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceRecord {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The substance membership score")]
        [DisplayName("Membership score")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Probability { get; set; }

    }
}
