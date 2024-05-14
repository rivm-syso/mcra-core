using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelTimeCourseSectionView : SectionView<KineticModelTimeCourseSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.InternalTargetSystemExposures.All(r => r.ExpressionType == null)) {
                hiddenProperties.Add("ExpressionType");
            }
            var routes = new List<ExposureRoute>();
            if (Model.InternalTargetSystemExposures.Any(r => r.Oral != null)) {
                routes.Add(ExposureRoute.Oral);
            } else {
                hiddenProperties.Add("Oral");
            }
            if (Model.InternalTargetSystemExposures.Any(r => r.Dermal != null)) {
                routes.Add(ExposureRoute.Dermal);
            } else {
                hiddenProperties.Add("Dermal");
            }
            if (Model.InternalTargetSystemExposures.Any(r => r.Inhalation != null)) {
                routes.Add(ExposureRoute.Inhalation);
            } else {
                hiddenProperties.Add("Inhalation");
            }
            if (routes.Count <= 1) {
                // If only one route, then hide route-specific fields
                hiddenProperties.Add("Oral");
                hiddenProperties.Add("Dermal");
                hiddenProperties.Add("Inhalation");
            }
            if (Model.InternalTargetSystemExposures.All(r => r.IndividualCode == null)) {
                hiddenProperties.Add("IndividualCode");
            }

            //no items in InternalTargetSystemExposures collection, return
            if ((Model.InternalTargetSystemExposures?.Count ?? 0) == 0) {
                return;
            }

            var groups = Model.InternalTargetSystemExposures
               .GroupBy(c => c.IndividualCode);
            if (groups.Count() > 1) {
                var targetPanelBuilder = new HtmlTabPanelBuilder();
                foreach (var group in groups) {
                    var targetPanelSb = new StringBuilder();
                    var panelBuilder = new HtmlTabPanelBuilder();
                    //loop over each item using a value tuple to select the item and the item's index
                    foreach (var record in group) {
                        if (record.MaximumTargetExposure > 0) {
                            var chartCreator = new KineticModelTimeCourseChartCreator(record, Model, record.Unit);
                            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                                name: $"KineticTimeCourse{group.Key}{record.BiologicalMatrix}",
                                section: Model,
                                items: Model.InternalTargetSystemExposures,
                                viewBag: ViewBag
                            );
                            var figCaption = !string.IsNullOrEmpty(record.IndividualCode)
                                ? $"PBK model time course {record.BiologicalMatrix} for individual {record.IndividualCode}."
                                : $"PBK model time course {record.BiologicalMatrix}.";
                            panelBuilder.AddPanel(
                                id: $"{record.BiologicalMatrix}",
                                title: $"{record.BiologicalMatrix}",
                                hoverText: record.BiologicalMatrix,
                                content: ChartHelpers.Chart(
                                    name: $"KineticTimeCourse{group.Key}{record.BiologicalMatrix}",
                                    section: Model,
                                    viewBag: ViewBag,
                                    chartCreator: chartCreator,
                                    fileType: ChartFileType.Svg,
                                    saveChartFile: true,
                                    caption: figCaption,
                                    chartData: percentileDataSection
                                )
                            );
                        }
                    }
                    panelBuilder.RenderPanel(targetPanelSb);
                    targetPanelBuilder.AddPanel(
                        id: $"{group.Key}",
                        title: $"Ind: {group.Key}",
                        hoverText: $"{group.Key}",
                        content: new HtmlString(targetPanelSb.ToString())
                    );
                }
                targetPanelBuilder.RenderPanel(sb);
            } else {
                var record = groups.First().First();
                var chartCreator = new KineticModelTimeCourseChartCreator(record, Model, record.Unit);
                sb.AppendChart(
                    "KineticTimeCourseChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    !string.IsNullOrEmpty(record.IndividualCode)
                        ? $"PBK model time course {record.BiologicalMatrix} for individual {record.IndividualCode}."
                        : $"PBK model time course {record.BiologicalMatrix}.",
                    true
                );
            }

            sb.AppendTable(
                Model,
                Model.InternalTargetSystemExposures,
                    "KineticModelDrilldownTable",
                    ViewBag,
                    caption: "Individual drilldown PBK model.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
        }
    }
}
