using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum NetworkAnalysisType {
        [Display(Name = "No network analysis")]
        [Description("No network analysis is applied.")]
        NoNetworkAnalysis,
        [Display(Name = "Apply network analysis")]
        [Description("Network analysis is applied on the substance x component (U) matrix.")]
        NetworkAnalysis,
    }
}
