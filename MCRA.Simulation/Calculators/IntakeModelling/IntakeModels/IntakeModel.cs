using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Base class for chronic exposure modeling like ISUF, LNN, LNN0, BBN, OIM.
    /// </summary>
    public abstract class IntakeModel : IIntakeModel {

        /// <summary>
        /// The exposure model type.
        /// </summary>
        public abstract IntakeModelType IntakeModelType { get; }

        /// <summary>
        /// Calculates the model parameters from the IndividualIntakes property.
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        public abstract void CalculateParameters(ICollection<SimpleIndividualDayIntake> individualDayIntakes);

        /// <summary>
        /// Calculates the usual exposures per cofactor-covariable grouping.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public abstract List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        );

        /// <summary>
        /// Calculates the usual exposures for a simulated set of individuals, incorporating a weighing factor based on cofactor/covariable counts per grouping.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public abstract List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed,
            CompositeProgressState progressState = null
        );

        /// <summary>
        /// Calculates the usual exposure for each individual in the IndividualDayIntakes
        /// collection.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public abstract List<ModelAssistedIntake> GetIndividualIntakes(int seed);

        /// <summary>
        /// These are all combinations of covariable and cofactor levels available in the data
        /// </summary>
        public List<CovariateGroup> DataBasedCovariateGroups { get; set; }

        /// <summary>
        /// These are the combinations of specified covariable levels using intervals (see settings) and specified extra levels (optional, see settings) 
        /// combined with all cofactor levels
        /// </summary>
        public List<CovariateGroup> SpecifiedPredictionCovariateGroups { get; set; }
    }
}
