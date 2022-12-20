using MCRA.Simulation.Calculators.IntakeModelling.Integrators;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public interface IAmountsModel {

        TransformBase GetDistribution(
            List<ModelledIndividualAmount> individualAmounts,
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        );

        ICollection<ModelledIndividualAmount> GetIndividualAmounts(int seed);

        ConditionalPredictionResults GetConditionalPredictions();
    }
}
