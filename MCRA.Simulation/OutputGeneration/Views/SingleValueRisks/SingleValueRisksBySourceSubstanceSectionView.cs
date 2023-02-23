using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksBySourceSubstanceSectionView : SectionView<SingleValueRisksBySourceSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }

            var distinctSubstances = Model.Records.Select(r => r.SubstanceCode).Distinct().Count();
            var distinctSources = Model.Records.GroupBy(r => (r.ExposureRoute, r.SourceCode)).Count();

            var description = $"Total {Model.Records.Count} risk estimates for exposure of {distinctSubstances} substances from {distinctSources} sources of exposure.";
            description += " The risk estimates are obtained by combining the exposure estimates with the hazard characterisation values.";

            sb.AppendDescriptionParagraph(description);
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueRisksBySourceTable",
                ViewBag,
                header: true,
                caption: "Single value risk estimates by source and substance.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
