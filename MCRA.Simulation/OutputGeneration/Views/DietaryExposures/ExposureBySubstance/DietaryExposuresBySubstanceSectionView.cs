using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryExposuresBySubstanceSectionView : SectionView<DietaryExposuresBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "SubstancePercentiles",
                Model, Model.SubstanceBoxPlotRecords,
                ViewBag,
                true,
                []
            );

            var chartCreator = new DietaryExposureBySubstanceBoxPlotChartCreator(Model);
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
