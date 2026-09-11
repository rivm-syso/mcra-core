using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PointsOfDepartureSummarySectionView : SectionView<PointsOfDepartureSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (!Model.Records?.Any() ?? true) {
                sb.AppendParagraph($"Note: no points of departure were found/selected.", "warning");

            } else {
                var hiddenProperties = new List<string>();

                if (Model.Records.All(r => string.IsNullOrEmpty(r.Code))) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.Code));
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.ModelCode));
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelEquation))) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.ModelEquation));
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.ParameterValues));
                }
                if (Model.Records.All(r => double.IsNaN(r.CriticalEffectSize))) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.CriticalEffectSize));
                }
                if (Model.Records.All(r => r.NumberOfUncertaintySets == 0)) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.NumberOfUncertaintySets));
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.Median));
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.Maximum));
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.Minimum));
                }
                // Use case: all PODs are provided for (external) dietary or other exposure routes, and not for internal biological matrix
                if (!Model.AllPodsAtTarget) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.BiologicalMatrix));
                }
                // Use case: all PODs are provided as (internal) values measured on a biological matrix, the exposure route is always the same, leave it out
                if (Model.AllPodsAtTarget) {
                    hiddenProperties.Add(nameof(PointsOfDepartureSummaryRecord.ExposureRoute));
                }

                var numSubstances = Model.Records.Select(r => r.CompoundCode).Distinct().Count();
                var numEffects = Model.Records.Select(r => r.EffectCode).Distinct().Count();

                var substanceString = (numSubstances != Model.Records.Count || numEffects > 1)
                    ? $" for {numSubstances} substances"
                    : string.Empty;
                var effectsString = numEffects > 1 ? $" and {numEffects} effects" : string.Empty;

                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} points of departure{substanceString}{effectsString}.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "PointsOfDeparturesRecordsTable",
                    ViewBag,
                    saveCsv: true,
                    caption: "Points of departure",
                    hiddenProperties: hiddenProperties);
            }
        }
    }
}
