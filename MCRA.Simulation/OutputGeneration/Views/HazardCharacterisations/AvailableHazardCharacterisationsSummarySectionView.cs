using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AvailableHazardCharacterisationsSummarySectionView : SectionView<AvailableHazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelEquation))) {
                hiddenProperties.Add("ModelEquation");
                hiddenProperties.Add("ModelParameterValues");
                hiddenProperties.Add("CriticalEffectSize");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add("ModelCode");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add("TargetDoseLowerBound");
                hiddenProperties.Add("TargetDoseUpperBound");
                hiddenProperties.Add("TargetDoseLowerBoundPercentile");
                hiddenProperties.Add("TargetDoseUpperBoundPercentile");
                hiddenProperties.Add("TargetDoseLowerBoundPercentileUnc");
                hiddenProperties.Add("TargetDoseUpperBoundPercentileUnc");
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add("GeometricStandardDeviation");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Organ))) {
                hiddenProperties.Add("Organ");
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalKineticConversionFactor) || r.NominalKineticConversionFactor == 1D)) {
                hiddenProperties.Add("NominalKineticConversionFactor");
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalInterSpeciesConversionFactor) || r.NominalInterSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add("NominalInterSpeciesConversionFactor");
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalIntraSpeciesConversionFactor) || r.NominalIntraSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add("NominalIntraSpeciesConversionFactor");
            }
            if (Model.Records.All(r => double.IsNaN(r.AdditionalConversionFactor) || r.AdditionalConversionFactor == 1D)) {
                hiddenProperties.Add("AdditionalConversionFactor");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.SystemExpressionType))) {
                hiddenProperties.Add("SystemExpressionType");
            }

            var failedRecordCount = Model.Records.Count(r => double.IsNaN(r.HazardCharacterisation));

            //Render HTML
            if (!Model.Records.Any()) {
                sb.AppendParagraph("No hazard characterisation data available.", "warning");
            }

            var validRecords = Model.Records;
            if (validRecords.Count > 1) {
                var recordsByTarget = Model.ChartRecords
                    .OrderBy(c => c.Key.BiologicalMatrix)
                    .ThenBy(c => c.Key.ExpressionType)
                    .ToList();
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var plotRecords in recordsByTarget) {
                    var unitKey = plotRecords.Key.Target.Code;
                    var numberOfRecords = plotRecords.Value.Count;
                    var targetUnit = plotRecords.Key;

                    var percentileDataSection = DataSectionHelper
                        .CreateCsvDataSection(
                            $"TargetDosesChart{unitKey}",
                            Model,
                            plotRecords.Value,
                            ViewBag
                        );

                    IReportChartCreator chartCreator = (validRecords.Count <= 30)
                        ? new AvailableHazardCharacterisationsChartCreator(
                                Model.SectionId,
                                plotRecords.Value,
                                targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType)
                            )
                        : new AvailableHazardCharacterisationsHistogramChartCreator(
                            Model.SectionId,
                            plotRecords.Value,
                            targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                            500,
                            350
                        );
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetUnit.BiologicalMatrix}_{targetUnit.ExpressionType}",
                        title: ComposePanelTabTitle(targetUnit),
                        hoverText: ComposeCaptionTitle(targetUnit, numberOfRecords),
                        content: ChartHelpers.Chart(
                            name: $"AvailableTargetDosesChart{unitKey}Chart",
                            section: Model,
                            viewBag: ViewBag,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true,
                            caption: ComposeCaptionTitle(targetUnit, numberOfRecords),
                            chartData: percentileDataSection
                        )
                    );
                }
                panelBuilder.RenderPanel(sb);
            }

            sb.AppendDescriptionParagraph($"Number of hazard characterisations: {Model.Records.Count}");
            sb.AppendTable(
                Model,
                Model.Records,
                "AvailableTargetDosesTable",
                ViewBag,
                caption: "Hazard characterisations information.",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
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
