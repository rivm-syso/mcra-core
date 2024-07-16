using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DustConcentrationDistributionsDataRecord {
        [DisplayName("Sample id")]
        public string idSample { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("The concentration of substance.")]
        [DisplayName("Concentration")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Concentration { get; set; }

        [Description("The concentration unit of the distribution.")]
        [DisplayName("Concentration unit")]
        public string Unit { get; set; }
    }
}
