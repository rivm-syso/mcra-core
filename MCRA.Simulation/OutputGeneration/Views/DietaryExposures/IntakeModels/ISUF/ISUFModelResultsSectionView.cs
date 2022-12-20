using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ISUFModelResultsSectionView : SectionView<ISUFModelResultsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var transform = string.Empty;
            var power = string.Empty;
            if (Model.Power > 0 && Model.Power < 1) {
                transform = "Transformation is power: ";
                power = Model.Power.ToString("G2");
            }
            if (double.IsNaN(Model.Power)) {
                transform = "Transformation is log ";
            }
            if (Model.Power == 11) {
                transform = "No transformation";
            }

            //Render HTML
            sb.AppendDescriptionParagraph("Only 2 days per individual are present in the dataset. The model fit will be unimodal " +
                      "and may be a too smooth representation of the real distribution.");
            var chartCreator0 = new ISUFDiscreteFrequencyDistributionChartCreator(Model);
            sb.AppendChart(
                "ISUFDiscreteFrequencyDistributionChart",
                chartCreator0,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator0.Title,
                true
            );
            sb.AppendDescriptionParagraph("Estimates of ISUF model");
            sb.AppendDescriptionParagraph($"Proportion of individuals with zero intake: {Model.DiscreteFrequencies[0].ToString("F1")}");
            sb.AppendDescriptionParagraph($"Unit variance between individuals: {Model.VarianceBetweenUnit.ToString("G4")}");
            sb.AppendDescriptionParagraph($"Unit variance within individuals: {Model.VarianceWithinUnit.ToString("G4")}");
            sb.AppendDescriptionParagraph($"Heterogeneity of variance between individuals (test statistic MA4) {Model.MA4.ToString("F2")}, (3 for homogeneous variances), p-value: {Model.PMA4.ToString("F2")}");
            sb.AppendDescriptionParagraph( transform + power);
            if (Model.NumberOfKnots > 0) {
                sb.AppendDescriptionParagraph($"Number of knots spline: {Model.NumberOfKnots}");
            }
            if (Model.ErrorRate < 0.85) {
                sb.AppendDescriptionParagraph($"normality rejected: p < { Model.ErrorRate.ToString("F2")}");
            } else {
                sb.AppendDescriptionParagraph($"normality not rejected: p >= {Model.ErrorRate.ToString("F2")}");
            }
            sb.AppendDescriptionParagraph($"Anderson-Darling (AD) test for normality: {Model.AndersonDarling.ToString("F2")}");

            sb.AppendDescriptionParagraph("Normal QQ-plot of transformed exposure amounts, after an adequate transformation to normality, " +
                "the fitted values (red dots) should approximately follow a straight line (solid line).");
            var chartCreator2 = new ISUFQQChartCreator(Model);
            sb.AppendChart(
                "ISUFQQChart",
                chartCreator0,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator2.Title,
                true
            );

            if (Model.ISUFDiagnostics.All(d => !double.IsNaN(d.GZ))) {
                sb.AppendDescriptionParagraph("For an adequate transformation to normality, the spline fit (solid line) " +
                    "should approximately follow the transformed exposure amounts (red dots).");
                var chartCreator3 = new ISUFSplineDiagnosticsChartCreator(Model);
                sb.AppendChart(
                    "ISUFSplineDiagnosticsChart",
                    chartCreator3,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator3.Title,
                    true
                );
            }
            var chartCreator1 = new ISUFDistributionChartCreator(Model);
            sb.AppendChart(
                "ISUFDistributionChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );
        }
    }
}
