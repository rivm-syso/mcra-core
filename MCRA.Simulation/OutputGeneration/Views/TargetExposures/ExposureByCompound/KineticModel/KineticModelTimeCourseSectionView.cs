using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelTimeCourseSectionView : SectionView<KineticModelTimeCourseSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.ExposureType == ExposureType.Acute) {
                hiddenProperties.Add("LongSubstanceAmount");
                hiddenProperties.Add("LongExposureAmount");
                hiddenProperties.Add("RatioLong");
            } else {
                hiddenProperties.Add("PeakSubstanceAmount");
                hiddenProperties.Add("PeakExposureAmount");
                hiddenProperties.Add("RatioPeak");
            }
            if (Model.DrilldownRecords.All(r => r.Oral == null)) {
                hiddenProperties.Add("Oral");
            }
            if (Model.DrilldownRecords.All(r => r.Dermal == null)) {
                hiddenProperties.Add("Dermal");
            }
            if (Model.DrilldownRecords.All(r => r.Inhalation == null)) {
                hiddenProperties.Add("Inhalation");
            }
            if (Model.DrilldownRecords.All(r => r.Dietary == null)) {
                hiddenProperties.Add("Dietary");
            }
            var targetConcentrationUnit = ViewBag.UnitsDictionary.ContainsKey("TargetConcentrationUnit") ? ViewBag.GetUnit("TargetConcentrationUnit") : ViewBag.GetUnit("KineticPerBWUnit");

            //no items in InternalTargetSystemExposures collection, return
            if ((Model.InternalTargetSystemExposures?.Count ?? 0) == 0) {
                return;
            }

            var groups = Model.InternalTargetSystemExposures
               .GroupBy(c => c.Code);
            var targetPanelBuilder = new HtmlTabPanelBuilder();
            foreach (var group in groups) {
                var targetPanelSb = new StringBuilder();
                var panelBuilder = new HtmlTabPanelBuilder();
                //loop over each item using a value tuple to select the item and the item's index
                foreach (var record in group) {
                    if (record.MaximumTargetExposure > 0) {
                        var chartCreator = new PBPKChartCreator(record, Model, targetConcentrationUnit);
                        var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                            name: $"KineticTimeCourse{group.Key}{record.Compartment}",
                            section: Model,
                            items: Model.DrilldownRecords,
                            viewBag: ViewBag
                        );
                        var figCaption = $"Compartment: {record.Compartment}";
                        panelBuilder.AddPanel(
                            id: $"{record.Compartment}",
                            title: $"{record.Compartment}",
                            hoverText: record.Compartment,
                            content: ChartHelpers.Chart(
                                name: $"KineticTimeCourse{group.Key}{record.Compartment}",
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

            sb.AppendTable(
                Model,
                Model.DrilldownRecords,
                    "KineticModelDrilldownTable",
                    ViewBag,
                    caption: "Individual drilldown PBK model.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
        }
    }
}
