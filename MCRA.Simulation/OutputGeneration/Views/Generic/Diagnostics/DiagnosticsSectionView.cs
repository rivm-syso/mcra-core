using MCRA.Simulation.OutputGeneration.Generic.Diagnostics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DiagnosticsSectionView : SectionView<DiagnosticsSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.MCSigmas?.Any() ?? false) {
                var percentages = Model.MCSigmas.Select(c => c.Percentage).Distinct().OrderBy(c => c).ToList();
                int take = 3;
                int loopCount = (int)Math.Ceiling(1.0 * percentages.Count() / take);

                //Render HTML
                var description = "Red dots: standard deviation (sd) of percentile estimates between subsets of simulations. Error bars indicate parametric 90% confidence interval for the sd.";
                if (Model.BootstrapSigmas.Count > 0) {
                    description += $" Blue dots: standard deviation of percentile estimates between uncertainty iterations of subsets of simulations. The red square indicates the specified number of iterations ({Model.BootstrapSize}) of an uncertainty run.";
                    description += $" Each blue dot represents the standard deviation of {Model.NumberOfUncertaintyRuns} percentiles.";
                }
                sb.AppendDescriptionParagraph(description);
                for (int i = 0; i < loopCount; i++) {
                    sb.Append($@"<table><thead><tr>");
                    foreach (var percentage in percentages.Skip(i * take).Take(take)) {
                        sb.Append($"<th>p{percentage.ToHtml()}</th>");
                    }
                    sb.Append("</tr></thead><tbody>");
                    sb.Append("<tr>");
                    foreach (var percentage in percentages.Skip(i * take).Take(take)) {
                        sb.Append($"<td>");
                        var mcVariances = Model.MCSigmas
                            .Where(c => c.Percentage == percentage)
                            .ToList();
                        var bootstrapVariances = Model.BootstrapSigmas.Count != 0
                            ? Model.BootstrapSigmas.Where(c => c.Percentage == percentage).ToList()
                            : null;
                        var chartCreator = new DiagnosticsChartCreator(mcVariances, 250, 300, percentage, Model.BootstrapSize, bootstrapVariances);
                        sb.AppendChart(
                            $"diagnostics_p{percentage}Chart",
                            chartCreator,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator.Title,
                            true
                        );
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                    sb.Append("</tbody></table>");
                }
            } else {
                sb.AppendDescriptionParagraph("No diagnostics available, number of iterations too small.");
            }
        }
    }
}
