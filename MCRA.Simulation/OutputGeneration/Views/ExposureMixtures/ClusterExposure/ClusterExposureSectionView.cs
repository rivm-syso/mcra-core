using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ClusterExposureSectionView : SectionView<ClusterExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                nameof(ClusterExposureRecord.IdCluster)
            };
            if (Model.Records.All(c => c.BiologicalMatrix == BiologicalMatrix.Undefined.ToString())) {
                hiddenProperties.Add(nameof(ClusterExposureRecord.BiologicalMatrix));
                hiddenProperties.Add(nameof(ClusterExposureRecord.ExpressionType));
            }
            if (Model.Records.All(c => c.pValue == string.Empty)) {
                hiddenProperties.Add(nameof(ClusterExposureRecord.pValue));
                hiddenProperties.Add(nameof(ClusterExposureRecord.MeanExposureOthers));
                hiddenProperties.Add(nameof(ClusterExposureRecord.NumberOfIndividualsOthers));
            }

            var description = string.Empty;
            if (Model.ClusterId == 0) {
                description = $"Population: exposure statistics ";
            } else {
                description = $"Subgroup {Model.ClusterId}, (n = {Model.NumberOfIndividuals}): exposure statistics ";
            }

            sb.AppendTable(
                Model,
                Model.Records,
                $"ClusterExposureInformationTable{Model.ClusterId}{Model.NumberOfIndividuals}",
                ViewBag,
                caption: description,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}