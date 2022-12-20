using MCRA.Utils.ProgressReporting;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public interface IUncorrelatedIntakeModel {

        void CalculateParameters(ICollection<SimpleIndividualDayIntake> individualDayAmounts);

        List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        );

        /// <summary>
        /// Calculates the usual exposure for each individual in the IndividualDayIntakes collection.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        List<ModelAssistedIntake> GetIndividualIntakes(int seed);

        /// <summary>
        /// Calculates the usual exposures for a simulated set of individuals, incorporating a 
        /// weighing factor based on cofactor/covariable counts per grouping.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed,
            CompositeProgressState progressState = null
        );
    }
}
