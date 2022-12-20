using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TestSystemsSummarySectionView : SectionView<TestSystemsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of test systems: {Model.Records?.Count ?? 0}");
            sb.AppendTable(
               Model,
               Model.Records,
               "TestSystemsRecordsTable",
               ViewBag,
               caption: "Test system summary.",
               saveCsv: true,
               header: true
            );
        }
    }
}
