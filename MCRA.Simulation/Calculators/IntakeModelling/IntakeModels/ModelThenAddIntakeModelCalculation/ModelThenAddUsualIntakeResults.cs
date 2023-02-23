using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {
    public sealed class ModelThenAddUsualIntakeResults {
        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes { get; set; }
        public List<ModelBasedIntakeResult> DietaryModelBasedIntakeResults { get; set; }
        public List<double> DietaryModelBasedIntakes {
            get {
                return DietaryModelBasedIntakeResults.SelectMany(c => c.ModelBasedIntakes).ToList();
            }
        }
    }
}
