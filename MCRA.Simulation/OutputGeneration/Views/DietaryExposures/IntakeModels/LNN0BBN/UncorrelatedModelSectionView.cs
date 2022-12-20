using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UncorrelatedModelSectionView : SectionView<UncorrelatedModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendParagraph("This section contains the results of fitting the two-part model. Part 1: frequencies, Part 2: amounts. No correlation between frequencies and amounts.");
        }
    }
}
