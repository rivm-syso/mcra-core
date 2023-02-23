using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardCharacterisationImputationCandidatesSectionView : SectionView<HazardCharacterisationImputationCandidatesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
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
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PotencyOrigin))) {
                hiddenProperties.Add("PotencyOrigin");
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
            if (Model.Records.All(r => double.IsNaN(r.ExpressionTypeConversionFactor) || r.ExpressionTypeConversionFactor == 1D)) {
                hiddenProperties.Add("ExpressionTypeConversionFactor");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelEquation))) {
                hiddenProperties.Add("ModelEquation");
                hiddenProperties.Add("ModelParameterValues");
                hiddenProperties.Add("CriticalEffectSize");
            }

            //Render HTML
            if (Model.Records.Any()) {
                var chartCreator = new HazardCharacterisationImputationCandidatesSectionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), 500, 350);
                sb.AppendChart(
                    "HazardCharacterisationImputationCandidatesSectionChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.Append("<table>");
                sb.AppendHeaderRow("", $"Harmonic mean ({ViewBag.GetUnit("IntakeUnit")})", $"p50 ({ViewBag.GetUnit("IntakeUnit")})", $"p95 ({ViewBag.GetUnit("IntakeUnit")})");
                if (Model.PercentilesCramerClassI?.Any() ?? false) {
                    sb.AppendTableRow("Cramer class I", $"{Model.HarmonicMeanCramerClassI:G3}", $"{Model.PercentilesCramerClassI[0]:G3}", $"{Model.PercentilesCramerClassI[1]:G3}");
                }
                if (Model.PercentilesCramerClassII?.Any() ?? false) {
                    sb.AppendTableRow("Cramer class II", $"{Model.HarmonicMeanCramerClassII:G3}", $"{Model.PercentilesCramerClassII[0]:G3}", $"{Model.PercentilesCramerClassII[1]:G3}");
                }
                if (Model.PercentilesCramerClassIII?.Any() ?? false) {
                    sb.AppendTableRow("Cramer class III", $"{Model.HarmonicMeanCramerClassIII:G3}", $"{Model.PercentilesCramerClassIII[0]:G3}", $"{Model.PercentilesCramerClassIII[1]:G3}");
                }
                if (Model.PercentilesAll?.Any() ?? false) {
                    sb.AppendTableRow("All", $"{Model.HarmonicMeanAll:G3}", $"{Model.PercentilesAll[0]:G3}", $"{Model.PercentilesAll[1]:G3}");
                }
                sb.Append("</table>");

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ImputedTargetDosesTable",
                    ViewBag,
                    caption: "Imputed target doses.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
