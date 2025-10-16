using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelTimeCourseSectionView : SectionView<PbkModelTimeCourseSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionTable(
               "KineticModelsSummaryRecordTable",
               Model.SectionId,
               Model.PbkModelSummaryRecord,
               ViewBag,
               caption: "PBK model.",
               header: true
            );

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
            if (Model.InternalTargetSystemExposures.All(r => double.IsNaN(r.Age))) {
                hiddenProperties.Add("Age");
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
                    var chartTimeCourse = string.Empty;
                    var chartBodyWeight = string.Empty;
                    var panelSb = new StringBuilder();
                    var panelBuilder = new HtmlTabPanelBuilder();
                    //loop over each item using a value tuple to select the item and the item's index
                    foreach (var record in group) {
                        if (record.MaximumTargetExposure > 0) {
                            var chartCreatorTimeCourse = new PbkModelTimeCourseChartCreator(record, Model, record.Unit);
                            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                                name: $"KineticTimeCourse{group.Key}{record.BiologicalMatrix}",
                                section: Model,
                                items: Model.InternalTargetSystemExposures,
                                viewBag: ViewBag
                            );
                            var figCaptionBiologicalMatrix = !string.IsNullOrEmpty(record.IndividualCode)
                                ? $"Internal concentration time series of {Model.PbkModelSummaryRecord.SubstanceName} in {record.BiologicalMatrix} for individual {record.IndividualCode}."
                                : $"Internal concentration time series of {Model.PbkModelSummaryRecord.SubstanceName} in {record.BiologicalMatrix}.";
                            chartTimeCourse = ChartHelpers.Chart(
                                    name: $"KineticTimeCourse{group.Key}{record.BiologicalMatrix}",
                                    section: Model,
                                    viewBag: ViewBag,
                                    chartCreator: chartCreatorTimeCourse,
                                    fileType: ChartFileType.Svg,
                                    saveChartFile: true,
                                    caption: figCaptionBiologicalMatrix,
                                    chartData: percentileDataSection
                                ).ToString();

                            var contentPanel = new HtmlString(
                                "<div class=\"figure-container\">"
                                + chartTimeCourse + chartBodyWeight
                                + "</div>"
                                + panelSb.ToString()
                            );
                            panelBuilder.AddPanel(
                                id: record.BiologicalMatrix,
                                title: record.BiologicalMatrix,
                                hoverText: record.BiologicalMatrix,
                                content: contentPanel
                            );
                        }
                    }
                    panelBuilder.RenderPanel(panelSb);
                    targetPanelBuilder.AddPanel(
                        id: group.Key,
                        title: $"Ind: {group.Key}",
                        hoverText: group.Key,
                        content: new HtmlString(panelSb.ToString())
                    );
                }
                targetPanelBuilder.RenderPanel(sb);
            } else {
                var record = groups.First().First();
                var chartCreator = new PbkModelTimeCourseChartCreator(record, Model, record.Unit);
                sb.AppendChart(
                    "KineticTimeCourseChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    !string.IsNullOrEmpty(record.IndividualCode)
                        ? $"Internal concentration time series of {Model.PbkModelSummaryRecord.SubstanceName} in {record.BiologicalMatrix} for individual {record.IndividualCode}."
                        : $"Internal concentration time series of {Model.PbkModelSummaryRecord.SubstanceName} in {record.BiologicalMatrix}.",
                    true
                );
            }

            sb.AppendTable(
                Model,
                Model.InternalTargetSystemExposures,
                $"PBKModelDrilldownTable_{Model.PbkModelSummaryRecord.SubstanceName}",
                ViewBag,
                caption: $"Individual drilldown PBK model simulations {Model.PbkModelSummaryRecord.SubstanceName}.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
