using MCRA.Simulation.OutputGeneration.Helpers;
using System;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CompoundExposureDistributionsSectionView : SectionView<CompoundExposureDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            int take = 4;
            int loopCount = (int)Math.Ceiling(1.0 * Model.CompoundExposureDistributionRecords.Count / take);

            //Render HTML
            sb.Append(@"<p class='note'>Note: this section summarizes the substance exposure distributions of the individual days.</p>
            <div class='tab-panel'>
                <ul class='tab-panel-header'>
                    <li class='active'><a data-toggle='tab' href='#A' title='rescaled X-Y axes'>Charts</a></li>
                    <li><a data-toggle='tab' href='#B' title='unscaled'>Table</a></li>
                    <li><a data-toggle='tab' href='#C' title='unscaled'>Combined distributions</a></li>
                    <li><a data-toggle='tab' href='#D' title='unscaled'>Exposures versus potency</a></li>
                </ul>
                <div class='tab-panel-content'>
                    <div id='A' class='tab-pane active'>
                        <table>
                            <tbody>");
            for (int i = 0; i < loopCount; i++) {
                sb.Append("<tr>");
                foreach (var record in Model.CompoundExposureDistributionRecords.Skip(i * take).Take(take)) {
                    sb.Append("<td>");
                    if (record.HistogramBins?.Any() ?? false) {
                        var chartCreator1 = new XYRescaledExposureDistributionPerCompoundChartCreator(record, 250, 175, Model.Upper, Model.Lower, Model.MaximumFrequency, ViewBag.GetUnit("IntakeUnit"));
                        sb.AppendChart(
                            "XYRescaledExposureDistributionPerCompoundChart",
                            chartCreator1,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator1.Title,
                            true
                        );
                    } else {
                        if (record.IsImputed == true) {
                            sb.Append($"<div class='no_measurements'>Imputed exposure for {record.CompoundName.ToHtml()}</div>");
                        } else {
                            sb.Append($"<div class='no_measurements'>No positive exposure for {record.CompoundName.ToHtml()}</div>");
                        }
                    }
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div><div id='B' class='tab-pane '>");
            if (Model.CompoundExposureDistributionRecords.Any()) {
                sb.AppendTable(
                       Model,
                       Model.CompoundExposureDistributionRecords,
                       "ExposureDistributionperSubstanceTable",
                       ViewBag,
                       caption: "Exposure statistics by modelled food (total distribution).",
                       saveCsv: true
                   );
            }
            sb.Append("</div>");
            if (Model.CombinedCompoundExposureDistributionRecord?.HistogramBins?.Any() ?? false) {
                sb.Append("<div id='C' class='tab-pane '>");
                if (Model.EqualityOfMeans && Model.CompoundExposureDistributionRecords.Any()) {
                    sb.AppendParagraph(" Means are unequal (p < 0.05, F-test)");
                }
                if (Model.HomogeneityOfVariances && Model.CompoundExposureDistributionRecords.Any()) {
                    sb.AppendParagraph(" Variances differ between substances (p < 0.05, Bartlett's test)");
                }
                var chartCreator1 = new UnrescaledExposureDistributionPerCompoundChartCreator(Model.CombinedCompoundExposureDistributionRecord, 350, 250, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "UnrescaledExposureDistributionPerSubstanceChart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator1.Title,
                    true
                );

                sb.Append("</div>");
            }
            sb.Append("<div id='D' class='tab-pane '><div>");
            var chartCreator = new CompoundPotencyVersusExposureChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "SubstancePotencyVersusExposureChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );

            sb.Append("</div></div></div></div>");
        }
    }
}
