using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InterSpeciesConversionModelsSummarySectionView : SectionView<InterSpeciesConversionModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => double.IsNaN(r.InterSpeciesFactorLowerBoundPercentile))) {
                hiddenProperties.Add("InterSpeciesFactorLowerBoundPercentile");
                hiddenProperties.Add("InterSpeciesFactorUpperBoundPercentile");
            }

            //Render HTML
            if (Model.DefaultInterSpeciesFactor != null) {
                sb.AppendDescriptionParagraph($"Default inter-species geometric mean: {Model.DefaultInterSpeciesFactor.GeometricMean}");
                sb.AppendDescriptionParagraph($"Default inter-species geometric standard deviation: {Model.DefaultInterSpeciesFactor.GeometricStDev}");
                //sb.AppendDescriptionParagraph($"Default standard human body weight ({Model.DefaultInterSpeciesFactor.HumanBodyWeightUnit}): {Model.DefaultInterSpeciesFactor.StandardHumanBodyWeight}");
                //sb.AppendDescriptionParagraph($"Default standard animal body weight ({Model.DefaultInterSpeciesFactor.AnimalBodyWeightUnit}): {Model.DefaultInterSpeciesFactor.StandardAnimalBodyWeight}");
            }

            sb.AppendDescriptionParagraph($"Number of inter species models: {Model.Records.Count}");
            sb.AppendTable(
                Model,
                Model.Records,
                "InterSpeciesConversionModelsTable",
                ViewBag,
                caption: "Interspecies conversion model summary.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
