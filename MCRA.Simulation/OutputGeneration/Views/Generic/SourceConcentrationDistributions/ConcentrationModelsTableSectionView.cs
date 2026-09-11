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
                nameof(ConcentrationModelRecord.FoodName),
                nameof(ConcentrationModelRecord.FoodCode)
            };

            var records = Model.Records;
            if (records.All(r => r.AgriculturalUseFraction == 1)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.AgriculturalUseFraction));
                hiddenProperties.Add(nameof(ConcentrationModelRecord.CorrectedAgriculturalUseFraction));
            }
            if (records.All(r => r.Mu == null)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.Mu));
                hiddenProperties.Add(nameof(ConcentrationModelRecord.Sigma));
            }
            if (records.All(r => r.MaximumResidueLimit == null)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.MaximumResidueLimit));
            }
            if (records.All(r => r.FractionOfMrl == null)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.FractionOfMrl));
            }
            if (records.All(r => string.IsNullOrEmpty(r.Warning))) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.Warning));
            }
            if (records.All(r => r.DesiredModel == r.Model)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.DesiredModel));
            }
            if (records.All(r => r.MeanConcentration == null)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.MeanConcentration));
            }
            if (records.All(r => double.IsNaN(r.FractionCensored))) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.FractionCensored));
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonDetects) && r.FractionNonDetects > 0)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.FractionNonDetects));
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonQuantifications) && r.FractionNonQuantifications > 0)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.FractionNonQuantifications));
            }
            if (records.All(r => r.AgriculturalUseFraction == r.CorrectedAgriculturalUseFraction)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.CorrectedAgriculturalUseFraction));
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.MeanConcentrationLowerBoundPercentile));
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationUpperBoundPercentile))) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.MeanConcentrationUpperBoundPercentile));
            }
            if (records.All(r => r.TotalMeasurementsCount == 0)) {
                hiddenProperties.Add(nameof(ConcentrationModelRecord.TotalMeasurementsCount));
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
