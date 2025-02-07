using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownSectionView : SectionView<DietaryChronicDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenPropertiesOverall = new List<string>();

            string label = Model.IsOIM ? " the Observed individual mean " : " the model assisted ";
            //Render HTML
            var description = $"Drilldown {Model.OverallIndividualDrillDownRecords.Count} individual days around " +
                $"{Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                $"of {label} exposure distribution.";

            //DietaryChronicDrillDownIndividualsSection
            if (Model.IsOIM) {
                hiddenPropertiesOverall.Add("FrequencyPrediction");
                hiddenPropertiesOverall.Add("AmountPrediction");
                hiddenPropertiesOverall.Add("MeanTransformedIntake");
                hiddenPropertiesOverall.Add("ShrinkageFactor");
                hiddenPropertiesOverall.Add("ModelAssistedExposure");
            }
            if (Model.OverallIndividualDrillDownRecords.All(c => c.Cofactor == string.Empty)) {
                hiddenPropertiesOverall.Add("Cofactor");
            }
            if (Model.OverallIndividualDrillDownRecords.All(c => c.Covariable == 0)) {
                hiddenPropertiesOverall.Add("Covariable");
            }
            sb.AppendTable(
                Model,
                Model.OverallIndividualDrillDownRecords,
                "DietaryChronicDrillDownIndividualsSectionTable",
                ViewBag,
                caption: description,
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenPropertiesOverall
            );
        }
    }
}

