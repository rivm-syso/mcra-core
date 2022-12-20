using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// For each combination of covariates (collection of cofactor/covariable
    /// combination for frequency/amount model combination) the percentiles
    /// and percentages are stored.
    /// </summary>
    public class CovariatesCollectionIntakePercentageSection {

        /// <summary>
        /// Combined cofactor and covariate levels for the frequency and amount model.
        /// </summary>
        public CovariatesCollection CovariatesCollection { get; set; }

        /// <summary>
        /// The intake percentage section for the covariates group.
        /// </summary>
        public IntakePercentageSection IntakePercentageSection { get; set; }
    }
}
