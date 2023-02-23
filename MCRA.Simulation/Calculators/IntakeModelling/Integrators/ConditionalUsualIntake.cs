namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// For each combination of covariates (collection of cofactor/covariable combination for frequency/amount model combination)
    /// the usual exposures are stored
    /// </summary>
    public class ConditionalUsualIntake {
        public CovariateGroup CovariateGroup { get; set; }
        public CovariatesCollection CovariatesCollection { get; set; }
        public List<double> ConditionalUsualIntakes { get; set; }
    }
}
