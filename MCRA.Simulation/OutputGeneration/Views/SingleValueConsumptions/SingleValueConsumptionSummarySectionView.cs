using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueConsumptionSummarySectionView : SectionView<SingleValueConsumptionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => double.IsNaN(r.LargePortion))) {
                hiddenProperties.Add("LargePortion");
            }
            if (Model.Records.All(r => double.IsNaN(r.MeanConsumption))) {
                hiddenProperties.Add("MeanConsumption");
            }
            if (Model.Records.All(r => double.IsNaN(r.MedianConsumption))) {
                hiddenProperties.Add("MedianConsumption");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ProcessingTypeCode))) {
                hiddenProperties.Add("ProcessingTypeCode");
                hiddenProperties.Add("ProcessingTypeName");
                hiddenProperties.Add("ProportionProcessing");
            }
            var description = $"Number of modelled food single value: {Model.Records.Count}.";
            sb.AppendDescriptionParagraph(description);
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueConsumptionsTable",
                ViewBag,
                caption: "Single value consumptions table.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
