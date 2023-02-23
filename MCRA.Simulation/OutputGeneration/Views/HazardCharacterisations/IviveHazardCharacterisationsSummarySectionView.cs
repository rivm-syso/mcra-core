using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IviveHazardCharacterisationsSummarySectionView : SectionView<IviveHazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var failedRecordCount = Model.Records.Where(r => double.IsNaN(r.HazardCharacterisation)).Count();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add("ModelCode");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add("TargetDoseLowerBoundPercentile");
                hiddenProperties.Add("TargetDoseUpperBoundPercentile");
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add("GeometricStandardDeviation");
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
            if (Model.Records.All(r => double.IsNaN(r.MolBasedRpf))) {
                hiddenProperties.Add("MolBasedRpf");
                hiddenProperties.Add("MolecularMass");
            }
            if (Model.Records.All(r => double.IsNaN(r.ExternalRpf))) {
                hiddenProperties.Add("ExternalRpf");
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
                caption: "Ivive hazard characterisations.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
