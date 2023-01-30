using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualsExposureSectionView : SectionView<IndividualsExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("ClusterId");

            sb.Append("<div class=\"figure-container\">");
            //Render HTML
           
            var chartCreatorPie = new ClusterPieChartCreator(
                Model.SectionId,
                Model.SubGroupComponentSummaryRecords,
                Model.ClusterId
            );
            sb.AppendChart(
                $"NMFIndividualsPieChart_{Model.ClusterId}",
                chartCreatorPie,
                ChartFileType.Svg,
                Model,
                ViewBag,
                $"Relative exposures to components in subgroup {Model.ClusterId}.",
                true
            );
            sb.AppendTable(
                Model,
                Model.SubGroupComponentSummaryRecords,
                $"IndividualsInformationTable{Model.ClusterId}",
                ViewBag,
                caption: $"Relative exposures to components in subgroup {Model.ClusterId}.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
            sb.Append("</div>");
        }
    }
}