using MCRA.Utils.Statistics;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Interface for the frequency model
    /// </summary>
    public interface IFrequencyModel {

        (Distribution, CovariateGroup) GetDistribution(
            ICollection<IndividualFrequency> individualFrequencies,
            CovariateGroup targetCovariateGroup
        );
        ICollection<IndividualFrequency> GetIndividualFrequencies();
        ConditionalPredictionResults GetConditionalPredictions();
    }
}
