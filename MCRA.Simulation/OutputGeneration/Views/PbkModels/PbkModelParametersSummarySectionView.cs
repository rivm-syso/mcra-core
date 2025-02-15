using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelParametersSummarySectionView : SectionView<PbkModelParametersSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "SubstanceCode",
                "SubstanceName"
            };
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Remark))) {
                hiddenProperties.Add("Remark");
            }

            var unmatchedParameters = Model.Records
                .GroupBy(r => r.ModelInstanceCode)
                .Where(r => r.Any(s => s.UnMatched))
                .Select(r => new {
                    ModelCode = r.Key,
                    ModelName = r.First().ModelInstanceName,
                    UnmatchedCount = r.Count(s => s.UnMatched)
                })
                .ToList();
            if (unmatchedParameters.Count > 1) {
                var totalCount = Model.Records.Count(r => r.Missing);
                sb.AppendNotification(
                    $"Warning: There are {unmatchedParameters.Count} model instances with parameters not defined in the model definition (in total {totalCount})." +
                    $" These parameters are not linked/used."
                );
            } else if (unmatchedParameters.Count == 1) {
                var instance = unmatchedParameters.First();
                sb.AppendNotification(
                    $"Warning: Model instance {instance.ModelName} ({instance.ModelCode}) has {instance.UnmatchedCount} parameters not defined in the model definition." +
                    $" These parameters are not linked/used."
                );
            }

            var instancesWithUnspecifiedParameters = Model.Records
                .GroupBy(r => r.ModelInstanceCode)
                .Where(r => r.Any(s => s.Missing))
                .Select(r => new {
                    ModelCode = r.Key,
                    ModelName = r.First().ModelInstanceName,
                    UnspecifiedCount = r.Count(s => s.Missing)
                })
                .ToList();
            if (instancesWithUnspecifiedParameters.Count > 1) {
                var totalCount = Model.Records.Count(r => r.Missing);
                sb.AppendNotification(
                    $"Note: There are {instancesWithUnspecifiedParameters.Count} model instances with unspecified parameters (in total {totalCount})." +
                    $" For these parameters, the default value from model definition is used."
                );
            } else if (instancesWithUnspecifiedParameters.Count == 1) {
                var instance = instancesWithUnspecifiedParameters.First();
                sb.AppendNotification(
                    $"Note: Model instance {instance.ModelName} ({instance.ModelCode}) has {instance.UnspecifiedCount} unspecified parameters." +
                    $" For these parameters, the default value from model definition is used."
                );
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "PBKModelParametersTable",
                ViewBag,
                header: true,
                caption: "PBK model parameters.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
