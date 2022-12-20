using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelThenAddIntakeModelsSection : SummarySection {

        public void SummarizeModels(
            SectionHeader header,
            CompositeIntakeModel compositeIntakeModel, 
            ActionData data 
        ) {
            int subOrder = 0;
            foreach (var intakeModel in compositeIntakeModel.PartialModels) {
                var title = $"Model {subOrder + 1}: {intakeModel.IntakeModel.IntakeModelType.GetDisplayAttribute().ShortName} (n={intakeModel.FoodsAsMeasured.Count})";
                var section = new ModelThenAddSummarySection();
                var subHeader = header.AddSubSectionHeaderFor(section, title, subOrder++);
                section.Summarize(subHeader, intakeModel, data);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
