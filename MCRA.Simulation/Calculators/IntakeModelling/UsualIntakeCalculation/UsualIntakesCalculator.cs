using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling.UsualIntakeCalculation {
    public class UsualIntakesCalculator {

        /// <summary>
        /// Computes and collects all usual intake output.
        /// </summary>
        /// <param name="intakeModel"></param>
        /// <param name="exposureType"></param>
        /// <param name="isCovariateModelling"></param>
        /// <param name="randomSeedModelBasedIntakesGeneration"></param>
        /// <param name="randomSeedModelAssisstedIntakesGeneration"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public UsualIntakeResults CalculateUsualIntakes(
            IntakeModel intakeModel,
            ExposureType exposureType,
            bool isCovariateModelling,
            int randomSeedModelBasedIntakesGeneration,
            int randomSeedModelAssisstedIntakesGeneration,
            CompositeProgressState progressState = null
        ) {
            if (intakeModel is IUncorrelatedIntakeModel) {
                var model = (intakeModel as IUncorrelatedIntakeModel);
                var result = new UsualIntakeResults {
                    ModelBasedIntakeResults = model.GetMarginalIntakes(randomSeedModelBasedIntakesGeneration)
                };
                if (exposureType == ExposureType.Chronic) {
                    result.IndividualModelAssistedIntakes = model.GetIndividualIntakes(randomSeedModelAssisstedIntakesGeneration);
                    if (result.IndividualModelAssistedIntakes != null) {
                        result.ModelAssistedIntakes = result.IndividualModelAssistedIntakes
                            .Select(c => new DietaryIndividualIntake() {
                                SimulatedIndividualId = c.SimulatedIndividualId,
                                Individual = c.Individual,
                                IndividualSamplingWeight = c.IndividualSamplingWeight,
                                DietaryIntakePerMassUnit = c.UsualIntake,
                            })
                            .ToList();
                    }
                }
                if (isCovariateModelling) {
                    result.ConditionalUsualIntakes = model
                        .GetConditionalIntakes(randomSeedModelBasedIntakesGeneration, progressState)
                        .OrderBy(c => c.CovariatesCollection.OverallCofactor, System.StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.OverallCovariable)
                        .ToList();
                }
                return result;
            } else {
                return null;
            }
        }
    }
}
