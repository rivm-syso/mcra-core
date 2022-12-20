using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TargetExposuresBySubstanceSectionView : SectionView<TargetExposuresBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "SubstancePercentiles",
                Model, Model.SubstanceBoxPlotRecords,
                ViewBag,
                true,
                new List<string>()
            );

            var chartCreator = new TargetExposurePercentilesBySubstanceBoxPlotChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "TotalDistributionSubstanceBoxPlotChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true,
                chartData: percentileDataSection
            );
        }
    }
}
