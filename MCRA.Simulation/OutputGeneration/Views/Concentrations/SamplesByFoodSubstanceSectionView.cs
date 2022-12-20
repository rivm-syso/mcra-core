using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SamplesByFoodSubstanceSectionView : SectionView<SamplesByFoodSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var description = $"Total {Model.ConcentrationInputDataRecords.Count} modelled food x substance combinations.";

            var hiddenProperties = new List<string>();
            if (Model.ConcentrationInputDataRecords.All(r => string.IsNullOrEmpty(r.MeasuredCompoundCode) || r.MeasuredCompoundCode == r.CompoundCode)) {
                hiddenProperties.Add("MeasuredCompoundCode");
                hiddenProperties.Add("MeasuredCompoundName");
            }
            if (Model.ConcentrationInputDataRecords.All(r => r.ZerosCount <= 0)) {
                hiddenProperties.Add("ZerosCount");
            }
            if (!Model.ConcentrationInputDataRecords.Any(r => r.Extrapolated)) {
                hiddenProperties.Add("Extrapolated");
            }
            if (Model.ConcentrationInputDataRecords.All(c => double.IsNaN((double)c.MeanLODs))){
                hiddenProperties.Add("MeanLODs");
            }
            if (Model.ConcentrationInputDataRecords.All(c => double.IsNaN((double)c.MeanLOQs))) {
                hiddenProperties.Add("MeanLOQs");
            }
            var focalCommodityRecords = Model.ConcentrationInputDataRecords.Where(r => r.FocalCombination);
            if (!focalCommodityRecords.Any()) {
                hiddenProperties.Add("FocalCombination");
            } else {
                if (focalCommodityRecords.Count() == 1) {
                    var combination = focalCommodityRecords.First();
                    description += $" Focal food x substance combination is {combination.FoodName}/{combination.CompoundName}.";
                } else {
                    description += $" Total {focalCommodityRecords.Count()} focal food x substance combinations.";
                }
                if (Model.HasAuthorisations) {
                    description += " Note that the focal combinations are always treated as authorised.";
                }
            }

            sb.AppendDescriptionParagraph(description);
            sb.AppendTable(
                Model,
                Model.ConcentrationInputDataRecords,
                "SamplesByFoodSubstanceTable",
                ViewBag,
                caption: "Sample statistics by modelled food x substance.",
                saveCsv: true,
                displayLimit: 20,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
