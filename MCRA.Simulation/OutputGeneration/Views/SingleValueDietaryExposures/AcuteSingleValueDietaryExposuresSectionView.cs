using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AcuteSingleValueDietaryExposuresSectionView : SectionView<AcuteSingleValueDietaryExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var records = Model.Records;

            if (records?.Count > 0) {
                if (records.All(r => string.IsNullOrEmpty(r.ProcessingTypeName))) {
                    hiddenProperties.Add("ProcessingTypeCode");
                    hiddenProperties.Add("ProcessingTypeName");
                    hiddenProperties.Add("ProcessingFactor");
                }

                if (records.All(r => r.OccurrenceFraction == 1)) {
                    hiddenProperties.Add("OccurrenceFraction");
                }
                var iestiMethod = Model.SingleValueDietaryExposuresCalculationMethod.GetDisplayName();
                if (Model.SingleValueDietaryExposuresCalculationMethod == SingleValueDietaryExposuresCalculationMethod.IESTINew) {
                    hiddenProperties.Add("CalculationMethod");
                    hiddenProperties.Add("UnitWeightEp");
                }

                var descriptions = new List<string>();
                var description = Model.Records.Count == 1
                    ? $"{iestiMethod} single value dietary exposure estimate"
                    : $"Total {Model.Records.Count} ({iestiMethod}) single value dietary exposure estimates";
                var svConcentrationsHeader = Toc?.GetSubSectionHeader<SingleValueConcentrationsSummarySection>();
                var svConsumptionsHeader = Toc?.GetSubSectionHeader<SingleValueConsumptionSummarySection>();
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
                    caption: $"Acute ({iestiMethod}) single value dietary exposure estimates by food and substance.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value exposures .", "warning");
            }
        }
    }
}
