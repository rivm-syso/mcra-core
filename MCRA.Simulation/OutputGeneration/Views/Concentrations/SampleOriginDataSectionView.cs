using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SampleOriginDataSectionView : SectionView<SampleOriginDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var distinctFoods = Model.SampleOriginDataRecords.Select(r => r.FoodCode).Distinct().Count();
            var distinctOrigins = Model.SampleOriginDataRecords.Select(r => r.Origin).Distinct().Count();
            sb.AppendDescriptionParagraph($"Number of samples per modelled foods x origin: {Model.SampleOriginDataRecords.Count}.");
            sb.AppendDescriptionParagraph($"Number of different foods: {distinctFoods}.");
            sb.AppendDescriptionParagraph($"Number of different origins: {distinctOrigins}.");
            //Render HTML
            sb.AppendTable(
               Model,
               Model.SampleOriginDataRecords,
               "SampleOriginTable",
               ViewBag,
               caption: "Sample statistics by modelled food.",
               saveCsv: true,
               header: true
            );
        }
    }
}
