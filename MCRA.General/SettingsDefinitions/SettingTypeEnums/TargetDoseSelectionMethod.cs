using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum TargetDoseSelectionMethod {
        [Display(Name = "Select most toxic", ShortName = "MostToxic")]
        [Description("Choose the most toxic (default).")]
        MostToxic,
        [Display(Name = "Take aggregate", ShortName = "Aggregate")]
        [Description("Choose an aggregated hazard characterisation when there there are multiple available candidates in nominal runs.")]
        Aggregate,
        [Display(Name = "Random draw", ShortName = "Draw")]
        [Description("Draw a random hazard characterisation.")]
        Draw,
    };
}
