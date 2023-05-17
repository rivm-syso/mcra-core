using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualsExposureSectionView : SectionView<IndividualsExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>() { "ClusterId", "NumberOfIndividuals" };
            var clusterIds = Model.SubGroupComponentSummaryRecords.Keys;
            const int columnCount = 3;
            sb.Append("<table><tbody><tr>");
            var i = 0;
            foreach (var clusterId in clusterIds) {
                if (i > 0 && i % columnCount == 0) {
                    sb.Append("</tr><tr>");
                }
                sb.Append("<td>");
                var records = Model.SubGroupComponentSummaryRecords[clusterId];
                var numberOfIndividuals = records.First().NumberOfIndividuals;
                var chartCreatorPie = new ClusterPieChartCreator(
                    Model.SectionId,
                    records,
                    clusterId
                );

                sb.AppendChart(
                    $"NMFIndividualsPieChart_{clusterId}",
                    chartCreatorPie,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    $"Relative exposures to components in subgroup {clusterId} (n={numberOfIndividuals}).",
                    true
                );

                sb.AppendTable(
                    Model,
                    records,
                    $"IndividualsInformationTable{clusterId}",
                    ViewBag,
                    caption: $"Relative exposures to components in subgroup {clusterId} (n={numberOfIndividuals}).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
                sb.Append("</td>");
                i++;
            }
            sb.Append("</tr></tbody></table>");
        }
    }
}