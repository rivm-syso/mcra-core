using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.Charting;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardCharacterisationsSummarySectionView : SectionView<HazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add("ModelCode");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EffectCode))) {
                hiddenProperties.Add("EffectCode");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.EffectName))) {
                hiddenProperties.Add("EffectName");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add("TargetDoseLowerBoundPercentile");
                hiddenProperties.Add("TargetDoseUpperBoundPercentile");
                hiddenProperties.Add("TargetDoseLowerBoundPercentileUnc");
                hiddenProperties.Add("TargetDoseUpperBoundPercentileUnc");
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add("GeometricStandardDeviation");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBound))) {
                hiddenProperties.Add("TargetDoseLowerBound");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseUpperBound))) {
                hiddenProperties.Add("TargetDoseUpperBound");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentileUnc))) {
                hiddenProperties.Add("TargetDoseLowerBoundPercentileUnc");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseUpperBoundPercentileUnc))) {
                hiddenProperties.Add("TargetDoseUpperBoundPercentileUnc");
            }
            if (Model.Records.All(r => string.Equals(r.BiologicalMatrix, BiologicalMatrix.Undefined.GetShortDisplayName(), StringComparison.OrdinalIgnoreCase))) {
                hiddenProperties.Add("BiologicalMatrix");
            }

            var failedRecordCount = Model.FailedRecordCount;
            var validRecords = Model.Records.Where(r => !double.IsNaN(r.HazardCharacterisation)).ToList();
            if (!validRecords.Any()) {
                sb.AppendWarning($"Note: failed to establish hazard characterisation for all {failedRecordCount} substances.");
            } else if (failedRecordCount > 0) {
                sb.AppendWarning($"Note: failed to establish hazard characterisation for {failedRecordCount} substances.");
            }

            var descriptions = new List<string>();
            if (Model.IsDistributionIntraSpecies) {
                descriptions.AddDescriptionItem($"Hazard characterisations are given as lognormal distributions of {Model.ExposureType.GetDisplayName().ToLower()} {Model.TargetDoseLevelType.GetDisplayName().ToLower()} doses.");
            } else {
                descriptions.AddDescriptionItem($"Hazard characterisations are given as {Model.ExposureType.GetDisplayName().ToLower()} {Model.TargetDoseLevelType.GetDisplayName().ToLower()} doses.");
            }

            if (Model.IsCompute) {
                var podHeader = Toc.GetSubSectionHeader<PointsOfDepartureSummarySection>();
                var drmHeader = Model.UseDoseResponseModels
                    ? Toc.GetSubSectionHeader<DoseResponseModelSection>()
                    : podHeader;

                descriptions.AddDescriptionItem(
                    $"Hazard characterisations are calculated from {{0}}.",
                    SectionReference.FromHeader(drmHeader),
                    SectionReference.FromHeader(podHeader)
                );

                if (Model.UseKineticModel) {
                    var descriptionUseKm = string.Empty;
                    if (Model.TargetDoseLevelType == TargetLevelType.External && Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds) {
                        descriptionUseKm = "Kinetic conversion was used to convert internal to external doses.";
                    } else if (Model.TargetDoseLevelType == TargetLevelType.Internal && Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVivoPods) {
                        descriptionUseKm = "Kinetic conversion was used to convert external to internal doses.";
                    } else if (Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms) {
                        descriptionUseKm = "Kinetic conversion was used to convert between internal and external doses.";
                    }
                    descriptions.AddDescriptionItem(descriptionUseKm);
                }
                if (Model.IsDistributionInterSpecies) {
                    descriptions.AddDescriptionItem($"Distributional model has been used for inter-species assessment factors.");
                }

                if (Model.IsDistributionIntraSpecies) {
                    descriptions.AddDescriptionItem($"Distributional model has been used for intra-species assessment factors.");
                }

                if (!Model.UseInterSpeciesConversionFactors) {
                    descriptions.AddDescriptionItem($"No inter-species conversion factors have been used in the hazard characterisation.");
                }
                if (!Model.UseIntraSpeciesConversionFactors && !Model.IsDistributionIntraSpecies) {
                    descriptions.AddDescriptionItem($"No intra-species factors have been used in the hazard characterisation.");
                }

                if (!Model.UseAssessmentFactor) {
                    descriptions.AddDescriptionItem($"No additional assessment factor has been used in the hazard characterisation.");
                } else {
                    descriptions.AddDescriptionItem($"The additional assessment factor used in the hazard characterisation is: {Model.AdditionalAssessmentFactor}.");
                }
                if (Model.UseInterSpeciesConversionFactors) {
                    if (double.IsNaN(Model.InterSpeciesConversionFactor)) {
                        descriptions.AddDescriptionItem(
                            "{0} were multiplied by an {1}, see second table in this section.",
                            SectionReference.FromHeader(podHeader),
                            SectionReference.FromActionTypeSettings(Toc, ActionType.InterSpeciesConversions, "inter-species conversion factor")
                        );
                    } else {
                        descriptions.AddDescriptionItem(
                            $"{{0}} were multiplied by an {{1}} =  {Model.InterSpeciesConversionFactor}.",
                            SectionReference.FromHeader(podHeader),
                            SectionReference.FromActionTypeSettings(Toc, ActionType.InterSpeciesConversions, "inter-species conversion factor")
                        );
                    }
                }
                if (Model.UseIntraSpeciesConversionFactors) {
                    if (double.IsNaN(Model.IntraSpeciesConversionFactor)) {
                        descriptions.AddDescriptionItem(
                            "{0} were multiplied by an {1}, see second table in this section.",
                            SectionReference.FromHeader(podHeader),
                            SectionReference.FromActionTypeSettings(Toc, ActionType.IntraSpeciesFactors, "intra-species conversion factor")
                        );
                    } else {
                        descriptions.AddDescriptionItem(
                            $"{{0}} were multiplied by an {{1}} =  {Model.IntraSpeciesConversionFactor}.",
                            SectionReference.FromHeader(podHeader),
                            SectionReference.FromActionTypeSettings(Toc, ActionType.IntraSpeciesFactors, "intra-species conversion factor")
                        );
                    }
                }
            } else {
                descriptions.AddDescriptionItem($"Hazard characterisations are read from data.");
            }
            sb.AppendDescriptionList(descriptions);

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

                    IChartCreator chartCreator = (validRecords.Count <= 30)
                        ? new HazardCharacterisationsChartCreator(
                                Model.SectionId,
                                plotRecords.Key.Target,
                                plotRecords.Value,
                                ViewBag.GetUnit(unitKey)
                            )
                        : new HazardCharacterisationsHistogramChartCreator(
                                    Model.SectionId,
                                    plotRecords.Value,
                                    ViewBag.GetUnit(unitKey),
                                    500,
                                    350
                        );
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetUnit.BiologicalMatrix}_{targetUnit.ExpressionType}",
                        title: ComposePanelTabTitle(targetUnit),
                        hoverText: ComposeCaptionTitle(targetUnit, numberOfRecords),
                        content: ChartHelpers.Chart(
                            name: $"TargetDosesChart{unitKey}Chart",
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

            // Table with hazard characterisation values
            if (validRecords.Any()) {
                sb.AppendTable(
                    Model,
                    validRecords,
                    "TargetDosesTable",
                    ViewBag,
                    caption: "Hazard characterisations.",
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
                title = $"{targetUnit.BiologicalMatrix.GetDisplayName()}";
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
