using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ErfDataSubgroupSummarySectionView : SectionView<ErfDataSubgroupSummarySection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncertaintyType))) {
                hiddenProperties.Add(nameof(ErfDataSubgroupSummaryRecord.ErSpecificationUncertaintyType));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncLower))) {
                hiddenProperties.Add(nameof(ErfDataSubgroupSummaryRecord.ErSpecificationUncLower));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncUpper))) {
                hiddenProperties.Add(nameof(ErfDataSubgroupSummaryRecord.ErSpecificationUncUpper));
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "ErfDataSubgroupSummaryTable",
                ViewBag,
                header: true,
                caption: "Exposure response functions subgroup information.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
