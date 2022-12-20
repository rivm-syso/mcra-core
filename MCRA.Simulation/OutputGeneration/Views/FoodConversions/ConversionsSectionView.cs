using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConversionsSectionView : SectionView<ConversionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Any() ?? false) {
                var description = $"Total {Model.Records.Count} conversion paths for {Model.NumberOfMatchedFoods} matched foods as eaten.";
                sb.AppendDescriptionParagraph(description);

                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.CompoundCode))) {
                    hiddenProperties.Add("CompoundCode");
                    hiddenProperties.Add("CompoundName");
                }
                if (Model.Records.All(r => double.IsNaN(r.MarketShare) || r.MarketShare == 1)) {
                    hiddenProperties.Add("MarketShare");
                }
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConversionTable",
                    ViewBag,
                    caption: "Conversion steps from food as eaten to modelled food.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("Failed to find conversions from food as eaten to modelled food.");
            }
        }
    }
}
