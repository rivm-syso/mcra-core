using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NominalTranslationProportionsSectionView : SectionView<NominalTranslationProportionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow("Measured substance code", "Measured substance name", "Proportion", "Substances and conversion factors");
            sb.Append("</thead><tbody>");
            foreach (var item in Model.Records) {
                sb.AppendRawTableRow(
                    item.MeasuredSubstanceCode.ToHtml(),
                    item.MeasuredSubstanceName.ToHtml(),
                    item.Proportion.ToString("G3"),
                    string.Join("<br/>", item.ActiveSubstanceCodes.Select((c, i) => $"<em>{item.ActiveSubstanceNames[i].ToHtml()} ({c.ToHtml()}) = {item.ConversionFactors[i]:G3}</em>"))
                );
            }
            sb.Append("</tbody></table>");
        }
    }
}
