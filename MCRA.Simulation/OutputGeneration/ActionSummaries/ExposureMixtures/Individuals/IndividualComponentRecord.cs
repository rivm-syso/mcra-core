using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualComponentRecord {
        [Description("Individual name.")]
        [DisplayName("Individual name")]
        public string Name  { get; set; }

        [Description("Normalized H matrix coefficient.")]
        [DisplayName("Weight")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double NmfValue { get; set; }

        public int IdComponent { get; set; }
    }
}
