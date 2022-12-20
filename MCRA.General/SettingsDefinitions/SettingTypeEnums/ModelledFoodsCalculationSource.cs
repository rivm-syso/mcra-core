using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// The source for the modelled foods derivation
    /// </summary>
    public enum ModelledFoodsCalculationSource {
        [Display(Name = "Derive modelled foods from concentrations", Order = 1)]
        [Description("Derive modelled foods from sample based concentration data.")]
        DeriveModelledFoodsFromSampleBasedConcentrations,

        [Display(Name = "Derive modelled foods from single value concentrations", Order = 2)]
        [Description("Derive modelled foods from single value concentrations.")]
        DeriveModelledFoodsFromSingleValueConcentrations,

        [Display(Name = "Derive modelled foods from concentration limits", Order = 3)]
        [Description("Derive modelled foods from concentration limits.")]
        UseWorstCaseValues,
    }
}
