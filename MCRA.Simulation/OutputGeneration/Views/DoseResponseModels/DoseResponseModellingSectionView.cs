using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseModellingSectionView : SectionView<DoseResponseModellingSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.DoseResponseModels.All(r => r.IsMixture)) {
                hiddenProperties.Add("IsMixture");
            }
            if (Model.DoseResponseModels.All(r => string.IsNullOrEmpty(r.ResponseUnit))) {
                hiddenProperties.Add("ResponseUnit");
            }
            hiddenProperties.Add("HasBenchmarkResponse");
            hiddenProperties.Add("BenchmarkResponseType");
            hiddenProperties.Add("BenchmarkResponse");
            hiddenProperties.Add("BenchmarkResponseUnit");
            if (Model.DoseResponseModels.All(r => r.LogLikelihood == null)) {
                hiddenProperties.Add("LogLikelihood");
            }
            if (Model.DoseResponseModels.All(r => r.ModelType == null)) {
                hiddenProperties.Add("ModelType");
            }
            if (Model.DoseResponseModels.All(r => string.IsNullOrEmpty(r.Message))) {
                hiddenProperties.Add("Message");
            }

            //Render HTML
            if (Model.DoseResponseModels?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.DoseResponseModels,
                    "DoseResponseModelsDetailsOverviewTable",
                    ViewBag,
                    caption: "Dose response models.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph(" No response data (models) available for the chosen effect ");
            }

            if (Model.EffectResponseCombinations?.Count > 0) {
                sb.AppendParagraph("Find below the selected response data and corresponding effect(s) for which no dose response models are fitted");
                sb.AppendTable(
                    Model,
                    Model.EffectResponseCombinations,
                    "EffectRepresentationTable",
                    ViewBag,
                    caption: "Effect representations.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
