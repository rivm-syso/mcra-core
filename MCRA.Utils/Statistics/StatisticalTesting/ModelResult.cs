using System.ComponentModel.DataAnnotations;

namespace MCRA.Utils.Statistics {

    public enum ErrorMessages {
        [Display(Name = "Successful convergence")]
        Convergence,
        [Display(Name = "Serious error, results are unreliable")]
        Error,
        [Display(Name = "No convergence is reached")]
        NoConvergence,
        [Display(Name = "Model is skipped")]
        ModelIsSkipped,
        [Display(Name = "Almost 100% positive, empirical frequency is used without dispersion")]
        ModelIsSkipped100Frequencies,
        [Display(Name = "Almost 100% zeroes, empirical frequency is used without dispersion")]
        ModelIsSkipped0Frequencies,
        [Display(Name = "All frequencies are the same, empirical frequency is used without dispersion")]
        ModelIsSkippedEqualFrequencies,
        [Display(Name = "Successful convergence, but failed to estimate standard errors")]
        ConvergenceNoStandardErrors,
    }

    public class ModelResult {
        public int DegreesOfFreedom { get; set; }
        public int DfPolynomial { get; set; }
        public double _2LogLikelihood { get; set; }
        public double FrequencyModelDispersion { get; set; }
        public double DispersionSe { get; set; }
        public List<double> Estimates { get; set; }
        public List<double> StandardErrors { get; set; }
        public ErrorMessages ErrorMessage { get; set; }
    }
}
