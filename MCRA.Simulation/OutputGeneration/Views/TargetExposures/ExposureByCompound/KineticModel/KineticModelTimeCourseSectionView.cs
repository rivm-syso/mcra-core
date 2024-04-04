using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

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

            var panelBuilder = new HtmlTabPanelBuilder();
            //loop over each item using a value tuple to select the item and the item's index
            foreach (var item in Model.InternalTargetSystemExposures) {
                if (item.MaximumTargetExposure > 0) {
                    var chartCreator = new PBPKChartCreator(item, Model, targetConcentrationUnit);
                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"KineticTimeCourse{item.Code}",
                        section: Model,
                        items: Model.DrilldownRecords,
                        viewBag: ViewBag
                    );
                    var figCaption = $"Model {Model.ModelCode}";
                    panelBuilder.AddPanel(
                        id: $"{item.Code}",
                        title: $"Id {item.Code}",
                        hoverText: item.Code,
                        content: ChartHelpers.Chart(
                            name: $"KineticTimeCourse{item.Code}",
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
            panelBuilder.RenderPanel(sb);

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
