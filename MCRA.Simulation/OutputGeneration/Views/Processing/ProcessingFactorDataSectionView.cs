using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ProcessingFactorDataSectionView : SectionView<ProcessingFactorDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Any() ?? false) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => double.IsNaN(r.Upper) || r.Nominal == r.Upper)) {
                    hiddenProperties.Add("Upper");
                }
                var description = $"Total {Model.Records.Count} processing factors.";
                if (Model.DuplicateEntryCount > 0) {
                    description += $" Found {Model.DuplicateEntryCount} duplicate processing factor definitions for the same food, substance and processing type";
                    if (Model.InconsistendEntryCount > 0) {
                        description += $" of which {Model.InconsistendEntryCount} were inconsistent (i.e., different factors for the same food, substance and processing type).";
                    } else {
                        description += $" which were all consistent (i.e., having the same factor).";
                    }
                }
                sb.AppendDescriptionParagraph(description);
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ProcessingFactorDataTable",
                    ViewBag,
                    header: true,
                    caption: "Processing factors summary.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No processing factors available.");
            }
        }
    }
}
