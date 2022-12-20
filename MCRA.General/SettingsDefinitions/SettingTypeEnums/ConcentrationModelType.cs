using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ConcentrationModelType {
        [Display(Name = "Empirical", ShortName = "Empirical")]
        [Description("Residues are sampled from the empirical distribution. Fallback: zero.")]
        Empirical,
        [Display(Name = "Censored value Spike LogNormal", ShortName = "CVSpike-LogN")]
        [Description("A lognormal model (logarithmic transformed values, with parameters mu and sigma^2) is fitted to the positive residues values. LOR information is not used. Fallback (if number of positives < 2): Empirical, but Maximum Residu Limit for pessimistic assessments.")]
        NonDetectSpikeLogNormal,
        [Display(Name = "Censored Spike Truncated LogNormal", ShortName = "CVSpike-TruncLogN")]
        [Description("A truncated lognormal model (with parameters mu and sigma^2) is fitted to the positive residues values. The LOR is used to estimate the truncated left tail of the distribution. Fallback: Lognormal non-detect spike.")]
        NonDetectSpikeTruncatedLogNormal,
        [Display(Name = "Censored LogNormal", ShortName = "CensLogN")]
        [Description("Advanced. A censored lognormal model (with parameters mu and sigma^2) is fitted to the censored and positives residue values. Note, this model is not available when agricultural use information is used. Fallback: Lognormal non-detect spike.")]
        CensoredLogNormal,
        [Display(Name = "Zero Spike Censored LogNormal", ShortName = "ZeroSpike-CensLogN")]
        [Description("Advanced. A mixture model with zero spike (p0) and censored lognormal model (with parameters mu and sigma^2) is fitted to the censored and positives residue values. Note, this model is not available when agricultural use information is used. Fallback: Censored lognormal.")]
        ZeroSpikeCensoredLogNormal,
        [Display(Name = "Censored Spike Maximum Residue Limit", ShortName = "CVSpike-MRL")]
        [Description("Censored Spike Maximum Residue Limit")]
        MaximumResidueLimit,
        [Display(Name = "Summary statistics", ShortName = "Summary statistic")]
        [Description("Summary statistics")]
        SummaryStatistics,
        [Display(Name = "LogNormal", ShortName = "LogN")]
        [Description("Lognormal model")]
        LogNormal,
    }
}
