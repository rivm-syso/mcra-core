using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                hiddenProperties.Add("TargetDoseLowerBoundPercentile");
                hiddenProperties.Add("TargetDoseUpperBoundPercentile");
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

            var failedRecordCount = Model.Records.Where(r => double.IsNaN(r.HazardCharacterisation)).Count();

            //Render HTML
            if (!Model.Records.Any()) {
                sb.AppendParagraph("No hazard characterisation data available.", "warning");
            }

            if (Model.Records.Count > 1 && Model.Records.Any(r => !double.IsNaN(r.HazardCharacterisation))) {
                if (Model.Records.Count <= 30) {
                    var chartCreator = new AvailableHazardCharacterisationsChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    sb.AppendChart(
                        "AvailableTargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                if (Model.Records.Count > 30) {
                    var chartCreator = new AvailableHazardCharacterisationsHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), 500, 350);
                    sb.AppendChart(
                        "AvailableTargetDosesChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            }

            sb.AppendDescriptionParagraph($"Number of characterisations: {Model.Records.Count}");
            sb.AppendTable(
                Model,
                Model.Records,
                "AvailableTargetDosesTable",
                ViewBag,
                caption: "Hazard characterisations information.",
                header: true,
                saveCsv: true
            );
        }
    }
}
