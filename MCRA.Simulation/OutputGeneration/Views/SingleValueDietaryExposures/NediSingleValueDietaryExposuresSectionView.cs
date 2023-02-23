using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NediSingleValueDietaryExposuresSectionView : SectionView<NediSingleValueDietaryExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Any() ?? false) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ProcessingTypeName))) {
                    hiddenProperties.Add("ProcessingTypeCode");
                    hiddenProperties.Add("ProcessingTypeName");
                    hiddenProperties.Add("ProcessingFactor");
                }
                if (Model.Records.All(r => r.OccurrenceFraction == 1)) {
                    hiddenProperties.Add("OccurrenceFraction");
                }

                var method = Model.Records.Select(r => r.CalculationMethod).Distinct();
                var descriptions = new List<string>();
                var description = Model.Records.Count == 1
                    ? $"Chronic single value dietary exposure estimate ({method.First()})"
                    : $"Total {Model.Records.Count} chronic {(method.Count() == 1 ? $"({method.First()}) " : string.Empty)}single value dietary exposure estimates";
                var svConcentrationsHeader = Toc.GetSubSectionHeader<SingleValueConcentrationsSummarySection>();
                var svConsumptionsHeader = Toc.GetSubSectionHeader<SingleValueConsumptionSummarySection>();
                descriptions.AddDescriptionItem(
                    description + " computed by combining {0} and {1}.",
                    SectionReference.FromHeader(svConsumptionsHeader),
                    SectionReference.FromHeader(svConcentrationsHeader)
                );
                sb.AppendDescriptionList(descriptions);

                sb.AppendTable(
                    Model, 
                    Model.Records,
                    "SingleValueDietaryExposuresTable",
                    ViewBag, 
                    header: true,
                    caption: "Chronic single value exposure estimates.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value exposures .", "warning");
            }
        }
    }
}
