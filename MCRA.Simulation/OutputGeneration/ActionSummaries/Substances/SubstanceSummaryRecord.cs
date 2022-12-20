using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstanceSummaryRecord {

        [DisplayName("Substance code")]
        [Description("Substance name")]
        public string CompoundCode { get; set; }

        [DisplayName("Substance name")]
        [Description("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Molecular weight.")]
        [Description("Molecular weight.")]
        [DisplayFormat(DataFormatString = "{0:G5}")]
        public double MolecularWeight { get; set; }

        [DisplayName("Cramer class")]
        [Description("Cramer class")]
        public string CramerClass { get; set; }

        [DisplayName("Is reference")]
        [Description("States whether this substance is the reference substance")]
        public bool IsReference { get; set; }

    }
}
