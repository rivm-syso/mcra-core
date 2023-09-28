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
                    hiddenProperties.Add("Code");
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                    hiddenProperties.Add("ModelCode");
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelEquation))) {
                    hiddenProperties.Add("ModelEquation");
                    hiddenProperties.Add("ParameterValues");
                }
                if (Model.Records.All(r => double.IsNaN(r.CriticalEffectSize))) {
                    hiddenProperties.Add("CriticalEffectSize");
                }
                if (Model.Records.All(r => r.NumberOfUncertaintySets == 0)) {
                    hiddenProperties.Add("NumberOfUncertaintySets");
                    hiddenProperties.Add("Median");
                    hiddenProperties.Add("Maximum");
                    hiddenProperties.Add("Minimum");
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
