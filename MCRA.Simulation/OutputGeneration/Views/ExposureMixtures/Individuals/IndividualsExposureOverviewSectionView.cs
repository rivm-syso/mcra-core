using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualsExposureOverviewSectionView : SectionView<IndividualsExposureOverviewSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("ClusterId");
            hiddenProperties.Add("Percentage");
            sb.Append("<div class=\"figure-container\">");
            var records = Model.SubgroupComponentSummaryRecords.Where(c => c.ClusterId == 1).Select(c => c).ToList();
            var chartCreatorPie = new PopulationPieChartCreator(
               Model.SectionId,
               records,
               0
           );

            sb.AppendChart(
                $"NMFIndividualsPieChart_{0}",
                chartCreatorPie,
                ChartFileType.Svg,
                Model,
                ViewBag,
                $"Relative exposure to components in population.",
                true
            );
            sb.AppendTable(
                Model,
                records,
                $"AllIndividualsInformationTable",
                ViewBag,
                caption: $"Relative exposure to components in population.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );

            sb.Append("</div>");

        }
    }
}