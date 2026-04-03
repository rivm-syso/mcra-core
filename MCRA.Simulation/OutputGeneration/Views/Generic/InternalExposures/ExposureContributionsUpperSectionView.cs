using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {

    public class ExposureContributionsUpperSectionView<S, T> : SectionView<InternalExposureContributionSectionBase<S, T>>
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new()
    {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;

            var panelBuilder = new HtmlTabPanelBuilder();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {                 
                hiddenProperties.Add("ContributionPercentage");
            }

            var individualString = Model.NumberOfIntakes == 1 ? $"1 individual" : $"{Model.NumberOfIntakes} individuals";
            sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({individualString}), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.Records.Count > 0) {
                var unstratifiedRecords = Model.Records.Where(r => string.IsNullOrEmpty(r.Stratification)).ToList();
                if (unstratifiedRecords.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
                    var chartCreator = new InternalExposureContributionPieChartCreator<S, T>(unstratifiedRecords, isUncertainty, Model.DescriptorName, true);
                    panelBuilder.AddPanel(
                        id: "Panel_unstratified",
                        title: "Unstratified",
                        hoverText: "Unstratified",
                        content: ChartHelpers.Chart(
                            name: $"UpperDistribution{Model.DescriptorKey}Chart",
                            section: Model,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: chartCreator.Title
                    ));
                }
                var stratifiedRecords = Model.Records.Where(r => !string.IsNullOrEmpty(r.Stratification)).ToList();
                if (stratifiedRecords.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
                    var chartCreator = new InternalExposureStackedBarChartCreator<S, T>(stratifiedRecords, isUncertainty, Model.DescriptorName, true);
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: ChartHelpers.Chart(
                            name: $"StratifiedUpperDistribution{Model.DescriptorKey}Chart",
                            section: Model,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: chartCreator.Title
                    ));

                }
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
                sb.AppendTable(
                    Model,
                    Model.Records,
                    $"ExposureBy{Model.DescriptorKey}UpperTable",
                    ViewBag,
                    caption: $"Contributions by {Model.DescriptorName} for the upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No upper distribution available for specified percentage");
            }
        }
    }
}

