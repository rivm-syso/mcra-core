
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresOutputData : IModuleOutputData {
        public FoodSurvey FoodSurvey { get; set; }
        public TargetUnit DietaryExposureUnit { get; set; }
        public IntakeModelType DesiredIntakeModelType { get; set; }
        public ICollection<ModelBasedIntakeResult> DietaryModelBasedIntakeResults { get; set; }
        public ICollection<DietaryIndividualDayIntake> DietaryIndividualDayIntakes { get; set; }
        public List<DietaryIndividualIntake> DietaryObservedIndividualMeans { get; set; }
        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes { get; set; }
        public List<ModelAssistedIntake> DrillDownDietaryIndividualIntakes { get; set; }
        public IIntakeModel DietaryExposuresIntakeModel { get; set; }
        public IDictionary<(Food Food, Compound Substance), double> TdsReductionFactors { get; set; }
        public ICollection<Food> TdsReductionScenarioAnalysisFoods { get; set; }
        public IModuleOutputData Copy() {
            return new DietaryExposuresOutputData() {
                FoodSurvey = FoodSurvey,
                DietaryExposureUnit = DietaryExposureUnit,
                DesiredIntakeModelType = DesiredIntakeModelType,
                DietaryModelBasedIntakeResults = DietaryModelBasedIntakeResults,
                DietaryIndividualDayIntakes = DietaryIndividualDayIntakes,
                DietaryObservedIndividualMeans = DietaryObservedIndividualMeans,
                DietaryModelAssistedIntakes = DietaryModelAssistedIntakes,
                DrillDownDietaryIndividualIntakes = DrillDownDietaryIndividualIntakes,
                DietaryExposuresIntakeModel = DietaryExposuresIntakeModel,
                TdsReductionFactors = TdsReductionFactors,
                TdsReductionScenarioAnalysisFoods = TdsReductionScenarioAnalysisFoods,
            };
        }
    }
}

