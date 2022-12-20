using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SamplePropertyDataSectionView : SectionView<SamplePropertyDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var propertyName = Model.PropertyName;
            var distinctFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
            var distinctPropertyValues = Model.Records.Select(r => r.PropertyValue).Distinct().Count();
            sb.AppendDescriptionParagraph($"Number of samples per foods x {propertyName}: {Model.Records.Count}.");
            sb.AppendDescriptionParagraph($"Number of different foods: {distinctFoods}.");
            sb.AppendDescriptionParagraph($"Number of different categories/values: {distinctPropertyValues}.");
            sb.AppendTable(
                Model, 
                Model.Records, 
                $"SamplePropertiesTable-{propertyName}",
                ViewBag,
                caption: $"Sample statistics by food and {propertyName}.",
                saveCsv: true
            );
        }
    }
}
