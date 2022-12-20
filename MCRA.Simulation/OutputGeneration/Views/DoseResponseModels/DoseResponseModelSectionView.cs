using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseModelSectionView : SectionView<DoseResponseModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.DoseResponseFits.All(r => string.IsNullOrEmpty(r.CovariateLevel))) {
                hiddenProperties.Add("CovariateLevel");
            }
            if (Model.DoseResponseFits.All(r => string.IsNullOrEmpty(r.ModelParameterValues))) {
                hiddenProperties.Add("ModelParameterValues");
            }
            if (Model.DoseResponseFits.Count <= 1) {
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("RpfLower");
                hiddenProperties.Add("RpfUpper");
                hiddenProperties.Add("RpfUncertainMedian");
                hiddenProperties.Add("RpfUncertainLowerBoundPercentile");
                hiddenProperties.Add("RpfUncertainUpperBoundPercentile");
            }
            if (!Model.DoseResponseFits.Any(r => r.BenchmarkDosesUncertain != null && r.BenchmarkDosesUncertain.Any())) {
                hiddenProperties.Add("BenchmarkDosesUncertainMedian");
                hiddenProperties.Add("BenchmarkDosesUncertainLowerBoundPercentile");
                hiddenProperties.Add("BenchmarkDosesUncertainUpperBoundPercentile");
            }
            if (!Model.DoseResponseFits.Any(r => r.RpfUncertain != null && r.RpfUncertain.Any())) {
                hiddenProperties.Add("RpfUncertainMedian");
                hiddenProperties.Add("RpfUncertainLowerBoundPercentile");
                hiddenProperties.Add("RpfUncertainUpperBoundPercentile");
            }
            if (Model.NumberOfBootstraps == null) {
                hiddenProperties.Add("NumberOfBootstraps");
            }
            if (string.IsNullOrEmpty(Model.Message)) {
                hiddenProperties.Add("Message");
            }

            //Render HTML
            sb.AppendDescriptionTable(
                Model.IdDoseResponseModel + "Summary",
                Model.SectionId,
                Model,
                ViewBag,
                header: false,
                hiddenProperties: hiddenProperties
            );
            sb.AppendTable(
                Model,
                Model.DoseResponseFits,
                $"ModelFit-{Model.IdDoseResponseModel}",
                ViewBag,
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );

            if (Model.DoseResponseSets.Any()) {
                var width = Model.DoseResponseSets.Count > 1 ? 600 : 500;
                var chartCreator = new DoseResponseFitChartCreator(Model, width, 350, false);
                sb.AppendChart(
                    $"ModelFit-{Model.IdDoseResponseModel}Chart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                if (Model.DoseResponseSets.Count > 1) {
                    chartCreator = new DoseResponseFitChartCreator(Model, width, 350, false);
                    sb.AppendChart(
                        $"ModelFit-{Model.IdDoseResponseModel}-RPF-ScaledChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            }
            if (Model.DoseResponseFits.Count > 1 && Model.DoseResponseFits.Any(r => !double.IsNaN(r.RelativePotencyFactor) && r.RpfLower != null)) {
                var chartCreator = new DoseResponseModelRpfsChartCreator(Model, false);
                sb.AppendChart(
                    $"ModelFit-{Model.IdDoseResponseModel}-RPFsChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
        }
    }
}
