using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductConcentrationModelsTableSectionView : SectionView<ConsumerProductConcentrationModelsTableSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var records = Model.Records;
            if (records.All(r => r.AgriculturalUseFraction == 1)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.AgriculturalUseFraction));
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.CorrectedAgriculturalUseFraction));
            }
            if (records.All(r => r.Mu == null)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.Mu));
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.Sigma));
            }
            if (records.All(r => r.MaximumResidueLimit == null)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.MaximumResidueLimit));
            }
            if (records.All(r => r.FractionOfMrl == null)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.FractionOfMrl));
            }
            if (records.All(r => string.IsNullOrEmpty(r.Warning))) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.Warning));
            }
            if (records.All(r => r.DesiredModel == r.Model)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.DesiredModel));
            }
            if (records.All(r => r.MeanConcentration == null)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.MeanConcentration));
            }
            if (records.All(r => double.IsNaN(r.FractionCensored))) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.FractionCensored));
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonDetects) && r.FractionNonDetects> 0)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.FractionNonDetects));
            }
            if (!records.Any(r => !double.IsNaN(r.FractionNonQuantifications) && r.FractionNonQuantifications > 0)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.FractionNonQuantifications));
            }
            if (records.All(r => r.AgriculturalUseFraction == r.CorrectedAgriculturalUseFraction)) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.CorrectedAgriculturalUseFraction));
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.MeanConcentrationLowerBoundPercentile));
            }
            if (records.All(r => double.IsNaN(r.MeanConcentrationUpperBoundPercentile))) {
                hiddenProperties.Add(nameof(ConsumerProductConcentrationModelRecord.MeanConcentrationUpperBoundPercentile));
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
                    .ThenBy(r => r.ConsumerProductName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.ConsumerProductCode, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                "ConsumerProductConcentrationModelsTable",
                ViewBag,
                caption: "Concentration model statistics by substance and consumer product",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
