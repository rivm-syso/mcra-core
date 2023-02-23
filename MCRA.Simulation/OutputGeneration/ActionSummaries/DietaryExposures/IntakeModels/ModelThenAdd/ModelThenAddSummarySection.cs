using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelThenAddSummarySection : ChronicIntakeEstimatesSection {

        public List<string> FoodNames { get; set; }
        public IntakeModelType IntakeModel { get; set; }

        public void Summarize(
            SectionHeader header,
            ModelThenAddPartialIntakeModel intakeModel,
            ActionData dataSource
        ) {
            FoodNames = intakeModel.FoodsAsMeasured.Select(r => r.Name).ToList();
            IntakeModel = intakeModel.IntakeModel.IntakeModelType;
            if (intakeModel.IntakeModel is BBNModel) {
                var bbnModel = intakeModel.IntakeModel as BBNModel;
                SummarizeModel(header, dataSource, bbnModel);
            }
            if (intakeModel.IntakeModel is LNN0Model) {
                var lnn0Model = intakeModel.IntakeModel as LNN0Model;
                SummarizeModel(header, dataSource, lnn0Model);
            }
            if (intakeModel.IntakeModel is ISUFModel || intakeModel.IntakeModel is LNNModel) {
                throw new Exception("Choose OIM, LNN0 or BBN; ISUF and LNN are not allowed");
            }
        }
    }
}
