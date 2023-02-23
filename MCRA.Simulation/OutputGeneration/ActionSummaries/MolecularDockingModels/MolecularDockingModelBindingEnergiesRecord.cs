using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelBindingEnergiesRecord {

        [Description("Molecular docking model code")]
        [DisplayName("Model code")]
        public string Code { get; set; }

        [Description("Molecular docking model name")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Description("Threshold")]
        [DisplayName("Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Threshold { get; set; }

        [Description("Number of receptors")]
        [DisplayName("Number of receptors")]
        public int? NumberOfReceptors { get; set; }

        [Display(AutoGenerateField = false)]
        public List<MolecularDockingModelCompoundRecord> BindingEnergies { get; set; }

    }
}
