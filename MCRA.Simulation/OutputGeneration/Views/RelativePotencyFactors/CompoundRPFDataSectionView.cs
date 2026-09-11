using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CompoundRPFDataSectionView : SectionView<CompoundRPFDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.Records.Any(r => r.IsUncertain)) {
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.RPFMean));
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.RPFMedian));
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.Range));
            }
            if (Model.Records.All(r => double.IsNaN(r.LimitDose))) {
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.LimitDose));
            }
            if (Model.Records.All(r => double.IsNaN(r.SystemHazardDose))) {
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.SystemHazardDose));
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.SystemToHumanConversionFactor));
            }
            if (Model.Records.All(r => r.PotencyOrigin == PotencyOrigin.Unknown)) {
                hiddenProperties.Add(nameof(CompoundRPFDataRecord.PotencyOrigin));
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
