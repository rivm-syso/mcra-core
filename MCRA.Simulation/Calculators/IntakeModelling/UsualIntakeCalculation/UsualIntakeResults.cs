using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class UsualIntakeResults {
        public List<ModelAssistedIntake> IndividualModelAssistedIntakes { get; set; }
        public List<ConditionalUsualIntake> ConditionalUsualIntakes { get; set; }
        public List<ModelBasedIntakeResult> ModelBasedIntakeResults { get; set; }
        public List<DietaryIndividualIntake> ModelAssistedIntakes { get; set; }
    }
}
