using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstanceComponentRecord {
        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName  { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Normalized W matrix coefficient.")]
        [DisplayName("Weight")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        [Display(AutoGenerateField = false)]
        public double NmfValue { get; set; }


        [Description("Contribution of substances.")]
        [DisplayName("Relative contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get { return NmfValue * 100; } }
    }
}
