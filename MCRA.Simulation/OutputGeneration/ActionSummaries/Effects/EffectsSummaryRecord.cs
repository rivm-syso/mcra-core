using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EffectsSummaryRecord {

        [DisplayName("Code")]
        [Description("The code of the effect.")]
        public string Code { get; set; }

        [DisplayName("Name")]
        [Description("Name of the effect.")]
        public string Name { get; set; }

        [DisplayName("Description")]
        [Description("Effect description.")]
        public string Description { get; set; }

        [DisplayName("Focal effect")]
        [Description("Specifies whether the effect is the focal effect of the analysis.")]
        public bool IsMainEffect { get; set; } = true;

        [DisplayName("Biological organisation")]
        [Description("Biological level at which the effect presents itself.")]
        public string BiologicalOrganisation { get; set; }

    }
}
