using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum FocalCommodityReplacementMethod {
        [Display(Name = "Replace samples with focal commodity samples", Order = 1)]
        [Description("Replace all samples of the selected focal commodity/commodities.")]
        ReplaceSamples = 0,
        [Display(Name = "Append focal commodity samples", Order = 2)]
        [Description("Add the samples of the focal commodity/commodities to the background concentration data.")]
        AppendSamples = 1,
        [Display(Name = "Replace measurements of focal food/substance combinations with measurements from focal commodity samples", Order = 3)]
        [Description("Replace the substance concentrations of the background concentrations by substance concentrations from the focal commodity concentration data.")]
        ReplaceSubstances = 2,
        [Display(Name = "Remove measurements of focal food/substance combinations", Order = 5)]
        [Description("Remove substance measurements for the selected focal food/substance combinations.")]
        MeasurementRemoval = 3,
        [Display(Name = "Replace measurements of focal food/substance combinations with concentration limit value", Order = 4)]
        [Description("Replace the substance concentrations of the background concentrations by a concentration limit value.")]
        ReplaceSubstanceConcentrationsByLimitValue = 4,
    }
}
