using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ComponentSelectionOverviewSectionView : SectionView<ComponentSelectionOverviewSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("Convergence");
            hiddenProperties.Add("Iteration");
            hiddenProperties.Add("Sparseness");

            var exposureData = "exposure data";
            if(Model.InternalConcentrationType == InternalConcentrationType.MonitoringConcentration) {
                exposureData = "human biomonitoring data";
            }
            var exposureTypeData = "individuals";
            if (Model.ExposureType == ExposureType.Acute) {
                exposureTypeData = "individual days";
            }
            //Render HTML
            if (Model.RatioCutOff > 0 || Model.TotalExposureCutOff > 0) {
                sb.AppendDescriptionParagraph($"The main components of {Model.NumberOfCompounds} substances were estimated by application of SNMU to {exposureData} of {Model.NumberOfSelectedDays} {exposureTypeData} (out of {Model.NumberOfDays}).");
                sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio cutoff value = {Model.RatioCutOff}, cumulative exposure cutoff value = {Model.TotalExposureCutOff} % ({Model.TotalExposureCutOffPercentile.ToString("G3")} {ViewBag.GetUnit("IntakeUnit")}).");
            } else {
                sb.AppendDescriptionParagraph($"The main components of {Model.NumberOfCompounds} substances were estimated by application of SNMU to {Model.NumberOfSelectedDays} exposure days.");
            }
            sb.AppendDescriptionParagraph($"Exposures are {Model.ExposureApproach.GetDisplayName().ToLower()}; {Model.Records.Count} components are estimated, sparseness constraint = {Model.Sparseness}.");
            sb.AppendDescriptionParagraph($"The variance explained by a component is an estimate of its relative contribution to the total exposure to all components in the data. The number of substances indicates how many substances contribute to a component. It is influenced by the chosen sparsity level in SNMU.");
            
            sb.AppendTable(
                Model,
                Model.Records,
                "MixturesInformationTable",
                ViewBag,
                caption: $"General characteristics of {Model.Records.Count} components.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );


            sb.AppendDescriptionParagraph($"The relative contribution of each substance to a component is visualized in the heatmap.");
            if (Model.SubstanceComponentRecords.Count > 1) {
                var chartCreator = new NMFHeatMapChartCreator(Model);
                sb.AppendChart(
                        "NMFHeatMapChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            } else {
                var chartCreator = new NMFSingleHeatMapChartCreator(Model);
                sb.AppendChart(
                        "NMFHeatMapChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            }

            sb.Append("<br/>");
        }
    }
}
