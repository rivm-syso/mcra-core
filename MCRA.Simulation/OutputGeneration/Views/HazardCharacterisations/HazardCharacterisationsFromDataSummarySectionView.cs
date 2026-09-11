using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardCharacterisationsFromDataSummarySectionView : SectionView<HazardCharacterisationsFromDataSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.ModelCode));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EffectCode))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.EffectCode));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EffectName))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.EffectName));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseLowerBoundPercentile));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseUpperBoundPercentile));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseLowerBoundPercentileUnc));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseUpperBoundPercentileUnc));
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.GeometricStandardDeviation));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBound))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseLowerBound));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseUpperBound))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseUpperBound));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentileUnc))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseLowerBoundPercentileUnc));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseUpperBoundPercentileUnc))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.TargetDoseUpperBoundPercentileUnc));
            }
            if (Model.Records.All(r => string.Equals(r.BiologicalMatrix, BiologicalMatrix.Undefined.GetShortDisplayName(), StringComparison.OrdinalIgnoreCase))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.BiologicalMatrix));
            }
            // Use case: all hazard characterisations are provided as (internal) values measured on a biological matrix, the exposure route is always the same, leave it out
            if (Model.AllHazardsAtTarget || Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.ExposureRoute));
            }
            if (Model.Records.All(r => r.NumberOfUncertaintySets == null || r.NumberOfUncertaintySets == 0)) {
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.NumberOfUncertaintySets));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.Median));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.Maximum));
                hiddenProperties.Add(nameof(HazardCharacterisationsFromDataSummaryRecord.Minimum));
            }

            // Table with hazard characterisation values
            sb.AppendTable(
                Model,
                Model.Records,
                "HazardCharacterisationsFromDataTable",
                ViewBag,
                caption: "Hazard characterisations obtained from data.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );

            if (Model.SubgroupRecords.All(r => r.NumberOfSubgroupsWithUncertainty == null || r.NumberOfSubgroupsWithUncertainty == 0)) {
                hiddenProperties.Add("TotalNumberOfUncertaintySets");
                hiddenProperties.Add("MinimumNumberUncertaintySets");
                hiddenProperties.Add("MaximumNumberUncertaintySets");
            }
            // Table and panel plot with hazard characterisation dependent on subgroups
            if (!Model.SubgroupRecords.All(r => r.NumberOfSubgroups == null)) {
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var item in Model.SubgroupPlotRecords) {
                    foreach (var set in item.Value) {
                        var chartCreator = new HCSubgroupChartCreator(Model, set);
                        var key = $"{item.Key.Target.GetDisplayName()}, {set.SubstanceName}";
                        panelBuilder.AddPanel(
                            id: $"Panel_{key}",
                            title: key,
                            hoverText: key,
                            content: ChartHelpers.Chart(
                                name: key,
                                section: Model,
                                viewBag: ViewBag,
                                chartCreator: chartCreator,
                                fileType: ChartFileType.Svg,
                                saveChartFile: true,
                                caption: chartCreator.Title
                            )
                        );
                    }
                }
                panelBuilder.RenderPanel(sb);

                sb.AppendTable(
                    Model,
                    Model.SubgroupRecords,
                    "HazardCharacterisationsSubgroupFromDataTable",
                    ViewBag,
                    caption: "Hazard characterisations subgroups obtained from data.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }

        private string ComposePanelTabTitle(TargetUnit targetUnit) {
            string title;
            if (targetUnit.TargetLevelType == TargetLevelType.External) {
                title = $"{targetUnit.ExposureRoute.GetShortDisplayName()} exposures ({targetUnit.GetShortDisplayName()})";
            } else {
                title = targetUnit.BiologicalMatrix.GetDisplayName();
                if (targetUnit.ExpressionType != ExpressionType.None) {
                    title += ", standardised";
                }
                title += $" ({targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType)})";
            }
            return title;
        }

        private string ComposeCaptionTitle(TargetUnit targetUnit, int numberOfRecords) {
            string title;
            if (targetUnit.TargetLevelType == TargetLevelType.External) {
                title = $"Hazard characterisations for {numberOfRecords} substances (in {targetUnit.GetShortDisplayName()}).";
            } else {
                title = $"Hazard characterisations for {numberOfRecords} substances in {targetUnit.BiologicalMatrix.GetDisplayName().ToLower()}";
                if (targetUnit.ExpressionType != ExpressionType.None) {
                    title += ", standardised";
                }
                title += $" ({targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType)}).";
            }
            return title;
        }
    }
}
