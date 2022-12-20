using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NetworkAnalysisSectionView : SectionView<NetworkAnalysisSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            //if (Model.RatioCutOff > 0 || Model.TotalExposureCutOff > 0) {
            //    sb.AppendDescriptionParagraph($"Number of substances {Model.NumberOfCompounds}, number of exposure days {Model.NumberOfSelectedDays} (out of {Model.NumberOfDays}).");
            //    sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio cutoff value = {Model.RatioCutOff}, cumulative exposure cutoff value = {Model.TotalExposureCutOff} % ({Model.TotalExposureCutOffPercentile.ToString("G3")} {ViewBag.GetUnit("IntakeUnit")}).");
            //} else {
            //    sb.AppendDescriptionParagraph($"Number of substances {Model.NumberOfCompounds}, number of exposure days {Model.NumberOfDays}");
            //}
            //sb.AppendDescriptionParagraph($"Exposures are {Model.ExposureApproach.GetDisplayName().ToLower()}; {Model.Records.Count} components are estimated, sparseness constraint = {Model.Sparseness}.");

            var chartCreator = new NetworkAnalysisChartCreator(Model);
            sb.AppendChart(
                    "NetworkAnalysisChart",
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
