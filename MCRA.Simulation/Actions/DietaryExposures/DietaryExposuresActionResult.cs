using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresActionResult : IActionResult {

        public TargetUnit DietaryExposureUnit { get; set; }
        public ICollection<SimulatedIndividualDay> SimulatedIndividualDays { get; set; }
        public ICollection<DietaryIndividualDayIntake> DietaryIndividualDayIntakes { get; set; }
        public Dictionary<Compound, List<ExposureRecord>> ExposurePerCompoundRecords { get; set; }
        public IntakeModelType DesiredIntakeModelType { get; set; }
        // Chronic intake modelling results
        public IIntakeModel IntakeModel { get; set; }
        public List<DietaryIndividualIntake> DietaryObservedIndividualMeans { get; set; }
        public List<ModelBasedIntakeResult> DietaryModelBasedIntakeResults { get; set; }
        public List<ConditionalUsualIntake> DietaryConditionalUsualIntakeResults { get; set; }
        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes { get; set; }
        public List<ModelAssistedIntake> IndividualModelAssistedIntakes { get; set; }
        public List<DriverSubstance> DriverSubstances{ get; set; }
        // Reduction factors TDS scenario analysis
        public IDictionary<(Food, Compound), double> TdsReductionFactors { get; set; }
        public ICollection<Food> TdsReductionScenarioAnalysisFoods { get; set; }
        public ExposureMatrix ExposureMatrix { get; set; }

        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
