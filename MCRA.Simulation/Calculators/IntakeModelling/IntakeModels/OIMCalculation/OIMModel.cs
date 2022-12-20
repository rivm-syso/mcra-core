using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation {

    /// <summary>
    /// Observed Individual Mean model for chronic exposure assessment
    /// </summary>
    public class OIMModel : IntakeModel {

        /// <summary>
        /// The exposure model type.
        /// </summary>
        public override IntakeModelType IntakeModelType {
            get {
                return IntakeModelType.OIM;
            }
        }

        public override void CalculateParameters(
            ICollection<SimpleIndividualDayIntake> individualDayAmounts) {
        }

        public override List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            return null;
        }

        public override List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed, 
            CompositeProgressState progressState = null
        ) {
            return null;
        }

        public override List<ModelAssistedIntake> GetIndividualIntakes(int seed) {
            return null;
        }
    }
}
