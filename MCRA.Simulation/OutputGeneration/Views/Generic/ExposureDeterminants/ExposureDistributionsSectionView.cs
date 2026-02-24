using MCRA.Simulation.OutputGeneration.ExposureDeterminants;
using MCRA.Simulation.OutputGeneration.Generic.ExposureDeterminants;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureDistributionsSectionView<Tsection> : SectionView<Tsection> 
        where Tsection : ExposureDistributionsDataSection{

        public override void RenderSectionHtml(StringBuilder sb) {
            var source = Model.Source.ToLower();

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} {source} {Model.Message} records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => c.AgeLower == null) &&
                    Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(BodyExposureFractionsDataRecord.idSubgroup));
                    hiddenProperties.Add(nameof(BodyExposureFractionsDataRecord.Sex));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(BodyExposureFractionsDataRecord.DistributionType));
                }
                if (Model.Records.All(c => !c.CvVariability.HasValue)) {
                    hiddenProperties.Add(nameof(BodyExposureFractionsDataRecord.CvVariability));
                }
                if (Model.Records.All(c => !c.AgeLower.HasValue)) {
                    hiddenProperties.Add(nameof(BodyExposureFractionsDataRecord.AgeLower));
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    $"{Model.Source}{Model.Message.Trim()}DataTable",
                    ViewBag,
                    caption: $"{Model.Source} {Model.Message}.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"No {source} {Model.Message} data available.");
            }
        }
    }
}
