using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// Expression types define the way in which a substance amount may be standardised, or possible other way expressed. For example, 
    /// a substance in urine may be expressed in terms of a creatinine standardisation.
    /// </summary>
    public enum ExpressionType {
        None,
        [Display(ShortName = "lipids", Name = "lipids", Description = "Standardise lipid soluble substances by total lipid content.")]
        Lipids,
        [Display(ShortName = "creatinine", Name = "creatinine", Description = "Standardise substance concentrations by creatinine content.")]
        Creatinine,
    };
}
