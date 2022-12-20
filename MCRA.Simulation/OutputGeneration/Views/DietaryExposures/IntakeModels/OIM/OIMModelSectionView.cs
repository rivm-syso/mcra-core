using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OIMModelSectionView : SectionView<OIMModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendParagraph("This section contains the results of the Observed Individual Means (OIM) model");
        }
    }
}
