using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationModelsTableSectionView : SectionView<ConcentrationModelsTableSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var records = Model.ConcentrationModelRecords;
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

            //Render HTML
            sb.Append($@"<table><tr>
                    <td>Number of records</td>
                    <td>{Model.ConcentrationModelRecords.Count}</td>
                </tr></table>");

            sb.AppendTable(
                Model,
                Model.ConcentrationModelRecords
                    .OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                "ConcentrationModelsTable",
                ViewBag,
                caption: "Concentration model statistics by substance and food",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
