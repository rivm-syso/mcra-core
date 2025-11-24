using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardisedTotalAttributableBod))) {
                hiddenProperties.Add("StandardisedTotalAttributableBod");
            }
            var isUncertainty = Model.Records.FirstOrDefault()?.TotalAttributableBods.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add("TotalAttributableBod");
                hiddenProperties.Add("StandardisedTotalAttributableBod");
            } else {
                hiddenProperties.Add("MedianTotalAttributableBod");
                hiddenProperties.Add("LowerTotalAttributableBod");
                hiddenProperties.Add("UpperTotalAttributableBod");
                hiddenProperties.Add("MedianStandardisedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardisedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardisedTotalAttributableBod");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.SourceIndicators))) {
                hiddenProperties.Add("SourceIndicators");
            }
            var recordsGrouped = Model.Records
                .GroupBy(r => (r.PopulationCode, r.SubstanceCode, r.EffectCode, r.BodIndicator));
            if (recordsGrouped.All(r => r.Count() <= 1)) {
                // Don't display ERF code/name unless multiple ERFs are used for same population,
                // substance, effect, and BoD indicator. 
                hiddenProperties.Add("ErfCode");
                hiddenProperties.Add("ErfName");
            }

            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add("StandardisedTotalAttributableBod");
                hiddenProperties.Add("MedianStandardisedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardisedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardisedTotalAttributableBod");
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
