using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardisedTotalAttributableBod))) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
            }
            var isUncertainty = Model.Records.FirstOrDefault()?.TotalAttributableBods.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalPopulationAttributableFraction));
            } else {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalPopulationAttributableFraction));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.SourceIndicators))) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.SourceIndicators));
            }
            var recordsGrouped = Model.Records
                .GroupBy(r => (r.PopulationCode, r.SubstanceCode, r.EffectCode, r.BodIndicator));
            if (recordsGrouped.All(r => r.Count() <= 1)) {
                // Don't display ERF code/name unless multiple ERFs are used for same population,
                // substance, effect, and BoD indicator. 
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.ErfCode));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.ErfName));
            }

            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperStandardisedTotalAttributableBod));
            }

            var caption = Model.IsCumulative
                ? "Environmental burden of disease overview (cumulative exposure)."
                : "Environmental burden of disease overview.";
            sb.AppendTable(
                Model,
                Model.Records,
                "EnvironmentalBurdenOfDiseaseSummaryTable",
                ViewBag,
                caption: caption,
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
