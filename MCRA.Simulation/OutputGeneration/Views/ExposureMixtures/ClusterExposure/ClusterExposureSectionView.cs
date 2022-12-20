using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ClusterExposureSectionView : SectionView<ClusterExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("IdCluster");
            if (Model.Records.All(c => c.pValue == string.Empty)) {
                hiddenProperties.Add("pValue");
                hiddenProperties.Add("MeanExposureOthers");
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