using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using static System.FormattableString;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PopulationPropertySummarySectionView : SectionView<PopulationPropertySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                nameof(PopulationPropertySummaryRecord.PopulationName),
                nameof(PopulationPropertySummaryRecord.PopulationCode),
                nameof(PopulationPropertySummaryRecord.Location)
            };
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.Description));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Location))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.Location));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.Description));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Value))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.Value));
            }
            if (Model.Records.All(r => r.MinValue == null || double.IsNaN((double)r.MinValue))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.MinValue));
            }
            if (Model.Records.All(r => r.MaxValue == null || double.IsNaN((double)r.MaxValue))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.MaxValue));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PropertyLevel))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.PropertyLevel));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.StartDate))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.StartDate));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EndDate))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.EndDate));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Type))) {
                hiddenProperties.Add(nameof(PopulationPropertySummaryRecord.Type));
            }

            if (!string.IsNullOrEmpty(Model.PopulationName)) {
                sb.AppendDescriptionParagraph($"Selected population: {Model.PopulationName} ({Model.PopulationCode})");
            } else {
                sb.AppendDescriptionParagraph($"Selected population: {Model.PopulationCode}");
            }

            if (double.IsNaN(Model.NominalPopulationBodyWeight)) {
                sb.AppendDescriptionParagraph($"No nominal population bodyweight specified.");
            } else {
                sb.AppendDescriptionParagraph($"Nominal population bodyweight: {Model.NominalPopulationBodyWeight} kg.");
            }

            if (double.IsNaN(Model.PopulationSize)) {
                sb.AppendDescriptionParagraph($"No population size specified.");
            } else {
                sb.AppendDescriptionParagraph($"Population size: {string.Format("{0:N0}", Model.PopulationSize)}.");
            }

            if (Model.Records.Count != 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "PopulationPropertyRecordsTable",
                    ViewBag,
                    caption: "Summary population properties.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties available.");
            }
        }
    }
}
