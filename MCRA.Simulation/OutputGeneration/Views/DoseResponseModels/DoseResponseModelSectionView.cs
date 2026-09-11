using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseModelSectionView : SectionView<DoseResponseModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.DoseResponseFits.All(r => string.IsNullOrEmpty(r.CovariateLevel))) {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.CovariateLevel));
            }
            if (Model.DoseResponseFits.All(r => string.IsNullOrEmpty(r.ModelParameterValues))) {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.ModelParameterValues));
            }
            if (Model.DoseResponseFits.Count <= 1) {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RelativePotencyFactor));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfLower));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUpper));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainMedian));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainLowerBoundPercentile));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainUpperBoundPercentile));
            }
            if (!Model.DoseResponseFits.Any(r => r.BenchmarkDosesUncertain != null && r.BenchmarkDosesUncertain.Any())) {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDosesUncertainMedian));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDosesUncertainLowerBoundPercentile));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDosesUncertainUpperBoundPercentile));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseLower));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseUpper));
            } else {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseLowerConfidenceLimit));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseUpperConfidenceLimit));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseLower));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.BenchmarkDoseUpper));
            }

            if (!Model.DoseResponseFits.Any(r => r.RpfUncertain != null && r.RpfUncertain.Any())) {
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainMedian));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainLowerBoundPercentile));
                hiddenProperties.Add(nameof(DoseResponseFitRecord.RpfUncertainUpperBoundPercentile));
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

            if (Model.DoseResponseSets?.Any() ?? false) {
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
                    chartCreator = new DoseResponseFitChartCreator(Model, width, 350, true);
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
