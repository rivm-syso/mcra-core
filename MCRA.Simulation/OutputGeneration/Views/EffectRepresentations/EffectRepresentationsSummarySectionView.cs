using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EffectRepresentationsSummarySectionView : SectionView<EffectRepresentationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.Records.Any(r => r.HasBenchmarkResponse)) {
                hiddenProperties.Add(nameof(EffectRepresentationRecord.BenchmarkResponseType));
                hiddenProperties.Add(nameof(EffectRepresentationRecord.BenchmarkResponse));
            } else if (Model.Records.All(r => r.HasBenchmarkResponse)) {
                hiddenProperties.Add(nameof(EffectRepresentationRecord.HasBenchmarkResponse));
            }
            var missingBenchmarkResponseCount = Model.Records.Count(r => !r.HasBenchmarkResponse);

            //Render HTML
            if (missingBenchmarkResponseCount > 0) {
                sb.AppendParagraph($"Note: there are {missingBenchmarkResponseCount} effect representations without a benchmark response specification.", "warning");
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "EffectRepresentationsTable",
                ViewBag,
                caption: "Effect representations",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
