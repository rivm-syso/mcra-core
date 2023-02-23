using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CompoundRPFDataSectionView : SectionView<CompoundRPFDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.Records.Any(r => r.IsUncertain)) {
                hiddenProperties.Add("RPFMean");
                hiddenProperties.Add("RPFMedian");
                hiddenProperties.Add("Range");
            }
            if (Model.Records.All(r => double.IsNaN(r.LimitDose))) {
                hiddenProperties.Add("LimitDose");
            }
            if (Model.Records.All(r => double.IsNaN(r.SystemHazardDose))) {
                hiddenProperties.Add("SystemHazardDose");
                hiddenProperties.Add("SystemToHumanConversionFactor");
            }
            if (Model.Records.All(r => r.PotencyOrigin == PotencyOrigin.Unknown)) {
                hiddenProperties.Add("PotencyOrigin");
            }

            //Render HTML
            if (Model.Records.Select(c => c.CompoundName).Distinct().Count() > 1) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SubstanceRPFDataTable",
                    ViewBag,
                    caption: "Relative potency factors statistics by substance.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No info available");
            }
        }
    }
}
