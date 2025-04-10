﻿using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureBySourceRouteSectionView : SectionView<ExternalExposureBySourceRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.Count > 0) {
                var chartCreator = new ExternalBoxPlotBySourceRouteChartCreator(
                    Model.BoxPlotRecords,
                    Model.ExposureUnit,
                    Model.ShowOutliers
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"ExternalBoxPlotBySourceRouteData",
                    section: Model,
                    items: Model.BoxPlotRecords,
                    viewBag: ViewBag
                );
                sb.AppendChart(
                    "ExternalBoxPlotBySourceRouteChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true,
                    chartData: percentileDataSection
                );
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ExternalExposureBySourceRouteTable",
                   ViewBag,
                   caption: $"External exposures statistics by source and route (total distribution).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No external exposures by source and route available.");
            }
        }
    }
}
