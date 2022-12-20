using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum DoseResponseModelType {
        [Description("Unknown")]
        [Display(Name = "Unknown", ShortName = "unknown")]
        Unknown = -1,
        [Description("Exp-m1")]
        [Display(Name = "Exp-m1", ShortName = "exp-m1")]
        Expm1,
        [Description("Exp-m2")]
        [Display(Name = "Exp-m2", ShortName = "exp-m2")]
        Expm2,
        [Description("Exp-m3")]
        [Display(Name = "Exp-m3", ShortName = "exp-m3")]
        Expm3,
        [Description("Exp-m4")]
        [Display(Name = "Exp-m4", ShortName = "exp-m4")]
        Expm4,
        [Description("Exp-m5")]
        [Display(Name = "Exp-m5", ShortName = "exp-m5")]
        Expm5,
        [Description("Hill-m1")]
        [Display(Name = "Hill-m1", ShortName = "hill-m1")]
        Hillm1,
        [Description("Hill-m2")]
        [Display(Name = "Hill-m2", ShortName = "hill-m2")]
        Hillm2,
        [Description("Hill-m3")]
        [Display(Name = "Hill-m3", ShortName = "hill-m3")]
        Hillm3,
        [Description("Hill-m4")]
        [Display(Name = "Hill-m4", ShortName = "hill-m4")]
        Hillm4,
        [Description("Hill-m5")]
        [Display(Name = "Hill-m5", ShortName = "hill-m5")]
        Hillm5,
        [Description("TwoStage")]
        [Display(Name = "TwoStage", ShortName = "TwoStage")]
        TwoStage,
        [Description("LogLogist")]
        [Display(Name = "LogLogist", ShortName = "LogLogist")]
        LogLogist,
        [Description("Weibull")]
        [Display(Name = "Weibull", ShortName = "Weibull")]
        Weibull,
        [Description("LogProb")]
        [Display(Name = "LogProb", ShortName = "LogProb")]
        LogProb,
        [Description("Gamma")]
        [Display(Name = "Gamma", ShortName = "Gamma")]
        Gamma,
        [Description("Logistic")]
        [Display(Name = "Logistic", ShortName = "Logistic")]
        Logistic,
        [Description("Probit")]
        [Display(Name = "Probit", ShortName = "Probit")]
        Probit,
        [Description("LVM_Exp m2")]
        [Display(Name = "LVM Exp m2", ShortName = "LVM Exp m2")]
        LVM_Exp_M2,
        [Description("LVM_Exp m3")]
        [Display(Name = "LVM Exp m3", ShortName = "LVM Exp m3")]
        LVM_Exp_M3,
        [Description("LVM_Exp m4")]
        [Display(Name = "LVM Exp m4", ShortName = "LVM Exp m4")]
        LVM_Exp_M4,
        [Description("LVM_Exp m5")]
        [Display(Name = "LVM Exp m5", ShortName = "LVM Exp m5")]
        LVM_Exp_M5,
        [Description("LVM_Hill m2")]
        [Display(Name = "LVM Hill m2", ShortName = "LVM Hill m2")]
        LVM_Hill_M2,
        [Description("LVM_Hill m3")]
        [Display(Name = "LVM Hill m3", ShortName = "LVM Hill m3")]
        LVM_Hill_M3,
        [Description("LVM_Hill m4")]
        [Display(Name = "LVM Hill m4", ShortName = "LVM Hill m4")]
        LVM_Hill_M4,
        [Description("LVM_Hill m5")]
        [Display(Name = "LVM Hill m5", ShortName = "LVM Hill m5")]
        LVM_Hill_M5,
        //[Description("Dose additive (SINGMOD)")]
        //[Display(Name = "Dose addition", ShortName = "Dose addition")]
        //DoseAddition,
    }
}
