using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IviveHazardCharacterisationsSummarySectionView : SectionView<IviveHazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var failedRecordCount = Model.Records.Count(r => double.IsNaN(r.HazardCharacterisation));
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.ModelCode));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.BiologicalMatrix));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseUpperBound))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseUpperBound));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBound))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseLowerBound));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseLowerBoundPercentile));
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseUpperBoundPercentile));
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseLowerBoundPercentileUnc));
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.TargetDoseUpperBoundPercentileUnc));
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.GeometricStandardDeviation));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalInterSpeciesConversionFactor) || r.NominalInterSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.NominalInterSpeciesConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalIntraSpeciesConversionFactor) || r.NominalIntraSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.NominalIntraSpeciesConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.AdditionalConversionFactor) || r.AdditionalConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.AdditionalConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.InternalMolBasedRpf))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.InternalMolBasedRpf));
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.MolecularMass));
            }
            if (Model.Records.All(r => double.IsNaN(r.ExternalRpf))) {
                hiddenProperties.Add(nameof(IviveHazardCharacterisationsSummaryRecord.ExternalRpf));
            }

            //Render HTML
            if (failedRecordCount > 0) {
                sb.AppendParagraph($"Error: failed to compute {failedRecordCount} hazard characterisations.", "warning");
            } else if (!Model.Records.Any()) {
                sb.AppendParagraph("Error: no hazard characterisations derived from IVIVE.", "warning");
            }
            if (Model.Records.Count > 1 && Model.Records.Any(r => !double.IsNaN(r.HazardCharacterisation))) {
                if (Model.Records.Count <= 30) {
                    var chartCreator = new IviveHazardCharacterisationsChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    sb.AppendChart(
                        "IviveTargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                if (Model.Records.Count > 30) {
                    var chartCreator = new IviveTargetDosesHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), 500, 350);
                    sb.AppendChart(
                        "IviveTargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "IviveTargetDosesTable",
                ViewBag,
                caption: $"Ivive hazard characterisations are given as {Model.TargetLevelType.GetDisplayName().ToLower()}s.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
