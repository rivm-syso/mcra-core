using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationModelsTableSectionView<Tsection> : SectionView<Tsection>
        where Tsection : ConcentrationModelsBase {
        public override void RenderSectionHtml(StringBuilder sb) {
            var typeName = typeof(Tsection).Name;
            const string suffix = "Section";
            if (typeName.EndsWith(suffix, StringComparison.Ordinal)) {
                typeName = typeName[..^suffix.Length];
            }

            var hiddenProperties = new List<string> {
                "FoodName",
                "FoodCode"
            };

            var records = Model.Records;
            if (records.All(r => r.AgriculturalUseFraction == 1)) {
                hiddenProperties.Add("AgriculturalUseFraction");
                hiddenProperties.Add("CorrectedAgriculturalUseFraction");
            }
            if (records.All(r => r.Mu == null)) {
                hiddenProperties.Add("Mu");
                hiddenProperties.Add("Sigma");
            }
            if (records.All(r => r.MaximumResidueLimit == null)) {
                hiddenProperties.Add("MaximumResidueLimit");
            }
            if (records.All(r => r.FractionOfMrl == null)) {
                hiddenProperties.Add("FractionOfMrl");
            }
            if (records.All(r => string.IsNullOrEmpty(r.Warning))) {
                hiddenProperties.Add("Warning");
            }
            if (records.All(r => r.DesiredModel == r.Model)) {
                hiddenProperties.Add("DesiredModel");
            }
            if (records.All(r => r.MeanConcentration == null)) {
                hiddenProperties.Add("MeanConcentration");
            }
            if (records.All(r => double.IsNaN(r.FractionCensored))) {
                hiddenProperties.Add("FractionCensored");
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonDetects) && r.FractionNonDetects > 0)) {
                hiddenProperties.Add("FractionNonDetects");
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonQuantifications) && r.FractionNonQuantifications > 0)) {
                hiddenProperties.Add("FractionNonQuantifications");
            }
            if (records.All(r => r.AgriculturalUseFraction == r.CorrectedAgriculturalUseFraction)) {
                hiddenProperties.Add("CorrectedAgriculturalUseFraction");
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationLowerBoundPercentile))) {
                hiddenProperties.Add("MeanConcentrationLowerBoundPercentile");
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationUpperBoundPercentile))) {
                hiddenProperties.Add("MeanConcentrationUpperBoundPercentile");
            }
            if (records.All(r => r.TotalMeasurementsCount == 0)) {
                hiddenProperties.Add("TotalMeasurementsCount");
            }


            //Render HTML
            sb.Append($@"<table><tr>
                    <td>Number of records</td>
                    <td>{Model.Records.Count}</td>
                </tr></table>");

            sb.AppendTable(
                Model,
                Model.Records
                    .OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                $"{typeName}Table",
                ViewBag,
                caption: "Concentration model statistics by substance",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
