using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

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
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add("GeometricStandardDeviation");
            }

            var failedRecordCount = Model.Records.Count(r => double.IsNaN(r.HazardCharacterisation));
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
                    $"Hazard characterisations are calculated from {{0}}, {{1}} were of type {Model.PotencyOrigins} ", 
                    SectionReference.FromHeader(drmHeader), 
                    SectionReference.FromHeader(podHeader)
                );

                if (Model.UseKineticModel) {
                    var descriptionUseKm = string.Empty;
                    if (Model.TargetDoseLevelType == TargetLevelType.External && Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds) {
                        descriptionUseKm = "{0} were used to convert internal to external doses.";
                    } else if (Model.TargetDoseLevelType == TargetLevelType.Internal && Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVivoPods) {
                        descriptionUseKm = "{0} were used to convert external to internal doses.";
                    } else if (Model.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms) {
                        descriptionUseKm = "{0} were used to convert between internal and external doses.";
                    }
                    descriptions.AddDescriptionItem(descriptionUseKm, SectionReference.FromHeader(Toc.GetSubSectionHeader<KineticModelsSummarySection>()));
                }
                if (Model.IsDistributionInterSpecies) {
                    descriptions.AddDescriptionItem($"Distributional model has been used for inter-species assessment factors.");
                }
                if (Model.IsDistributionIntraSpecies) {
                    descriptions.AddDescriptionItem($"Distributional model has been used for intra-species assessment factors.");
                }
                if (!Model.UseInterSpeciesConversionFactors && !Model.UseIntraSpeciesConversionFactors) {
                    descriptions.AddDescriptionItem($"No assessment factors have been used in the hazard characterisation.");
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
                if (validRecords.Count <= 30) {
                    var chartCreator = new HazardCharacterisationsChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    sb.AppendChart(
                        "TargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                if (validRecords.Count > 30) {
                    var chartCreator = new HazardCharacterisationsHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), 500, 350);
                    sb.AppendChart(
                        "TargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            }

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
    }
}
