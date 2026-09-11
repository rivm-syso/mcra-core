using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardCharacterisationImputationCandidatesSectionView : SectionView<HazardCharacterisationImputationCandidatesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.ModelCode));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.TargetDoseLowerBoundPercentile));
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.TargetDoseUpperBoundPercentile));
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.TargetDoseLowerBoundPercentileUnc));
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.TargetDoseUpperBoundPercentileUnc));
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.GeometricStandardDeviation));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PotencyOrigin))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.PotencyOrigin));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Organ))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.Organ));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalKineticConversionFactor) || r.NominalKineticConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.NominalKineticConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalInterSpeciesConversionFactor) || r.NominalInterSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.NominalInterSpeciesConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalIntraSpeciesConversionFactor) || r.NominalIntraSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.NominalIntraSpeciesConversionFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.ExpressionTypeConversionFactor) || r.ExpressionTypeConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.ExpressionTypeConversionFactor));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelEquation))) {
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.ModelEquation));
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.ModelParameterValues));
                hiddenProperties.Add(nameof(HazardCharacterisationImputationRecord.CriticalEffectSize));
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
                if (Model.PercentilesCramerClassI?.Length > 0) {
                    sb.AppendTableRow("Cramer class I", $"{Model.HarmonicMeanCramerClassI:G3}", $"{Model.PercentilesCramerClassI[0]:G3}", $"{Model.PercentilesCramerClassI[1]:G3}");
                }
                if (Model.PercentilesCramerClassII?.Length > 0) {
                    sb.AppendTableRow("Cramer class II", $"{Model.HarmonicMeanCramerClassII:G3}", $"{Model.PercentilesCramerClassII[0]:G3}", $"{Model.PercentilesCramerClassII[1]:G3}");
                }
                if (Model.PercentilesCramerClassIII?.Length > 0) {
                    sb.AppendTableRow("Cramer class III", $"{Model.HarmonicMeanCramerClassIII:G3}", $"{Model.PercentilesCramerClassIII[0]:G3}", $"{Model.PercentilesCramerClassIII[1]:G3}");
                }
                if (Model.PercentilesAll?.Length > 0) {
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
