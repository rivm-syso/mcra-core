﻿using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureBySourceSectionView : SectionView<ExternalExposureBySourceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.Count > 0) {
                var chartCreator = new ExternalBoxPlotBySourceChartCreator(
                    Model.BoxPlotRecords,
                    Model.ExposureUnit,
                    Model.ShowOutliers
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"ExternalBoxPlotBySourceData",
                    section: Model,
                    items: Model.BoxPlotRecords,
                    viewBag: ViewBag
                );
                sb.AppendChart(
                    "ExternalBoxPlotBySourceChart",
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
                   "ExternalExposureBySourceTable",
                   ViewBag,
                   caption: $"External exposures statistics by source (total distribution).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No external exposures by source available.");
            }
        }
    }
}
