using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelRecord {

        [Description("QSAR model code")]
        [DisplayName("Model code")]
        public string Code { get; set; }

        [Description("QSAR model name")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Description("QSAR model description")]
        [DisplayName("Model description")]
        public string Description { get; set; }

        [Description("Effect name")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Effect code")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("Accuracy")]
        [DisplayName("Accuracy")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Accuracy { get; set; }

        [Description("Sensitivity")]
        [DisplayName("Sensitivity")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Sensitivity { get; set; }

        [Description("Specificity")]
        [DisplayName("Specificity")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Specificity { get; set; }

        [Description("Number of computed membership scores")]
        [DisplayName("Number of computed membership scores")]
        public double MembershipScoresCount { get; set; }

        [Description("Fraction of positive membership scores")]
        [DisplayName("Fraction positive")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FractionPositives { get; set; }

    }
}
