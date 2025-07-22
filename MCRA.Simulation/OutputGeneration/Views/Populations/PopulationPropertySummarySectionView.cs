using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using static System.FormattableString;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PopulationPropertySummarySectionView : SectionView<PopulationPropertySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "PopulationName",
                "PopulationCode",
                "Location"
            };
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                hiddenProperties.Add("Description");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Location))) {
                hiddenProperties.Add("Location");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                hiddenProperties.Add("Description");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Value))) {
                hiddenProperties.Add("Value");
            }
            if (Model.Records.All(r => r.MinValue == null || double.IsNaN((double)r.MinValue))) {
                hiddenProperties.Add("MinValue");
            }
            if (Model.Records.All(r => r.MaxValue == null || double.IsNaN((double)r.MaxValue))) {
                hiddenProperties.Add("MaxValue");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PropertyLevel))) {
                hiddenProperties.Add("PropertyLevel");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.StartDate))) {
                hiddenProperties.Add("StartDate");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EndDate))) {
                hiddenProperties.Add("EndDate");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Type))) {
                hiddenProperties.Add("Type");
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
                sb.AppendDescriptionParagraph(Invariant($"Population size: {Model.PopulationSize:N0}."));
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
