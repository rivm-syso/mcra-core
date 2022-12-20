using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelRecord {

        [Description("Molecular docking model code")]
        [DisplayName("Model code")]
        public string Code { get; set; }

        [Description("Molecular docking model name")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Description("Molecular docking model description")]
        [DisplayName("Model description")]
        [Display(AutoGenerateField = false)]
        public string Description { get; set; }

        [Description("Effect code")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("Effect name")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Threshold")]
        [DisplayName("Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Threshold { get; set; }

        [Description("Number of receptors")]
        [DisplayName("Number of receptors")]
        public int? NumberOfReceptors { get; set; }

        [Description("Number of computed binding energies")]
        [DisplayName("Number of computed binding energies")]
        public double BindingEnergiesCount { get; set; }

        [Description("Binding energies lower quartile")]
        [DisplayName("Binding energies lower quartile")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BindingEnergiesLowerQuartile { get; set; }

        [Description("Binding energies median")]
        [DisplayName("Binding energies median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BindingEnergiesMedian { get; set; }

        [Description("Binding energies upper quartile")]
        [DisplayName("Binding energies upper quartile")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BindingEnergiesUpperQuartile { get; set; }

        [Description("Min. binding energy")]
        [DisplayName("Min. binding energy")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BindingEnergiesMinimum { get; set; }

        [Description("Max. binding energy")]
        [DisplayName("Max. binding energy")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BindingEnergiesMaximum { get; set; }

    }
}
