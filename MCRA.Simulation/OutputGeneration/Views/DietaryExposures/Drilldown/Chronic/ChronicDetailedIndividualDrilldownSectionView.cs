using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ChronicDetailedIndividualDrilldownSectionView : SectionView<ChronicDetailedIndividualDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var processingCalculation = Model.IsProcessing ? "* Processing factor / Processing correction factor" : "";
            sb.Append($"Exposure per day = portion amount * concentration {processingCalculation.ToLower()} / {Model.BodyWeight} (= body weight)");
            var descriptionIndividual = $"Individual {Model.DrilldownRecord.IndividualId.ToHtml()}, body weight: {Model.BodyWeight} " +
                $"{ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, sampling weight: {Model.DrilldownRecord.SamplingWeight:F2}" +
                $" observed individual mean: {Model.IndividualMean:G3} {ViewBag.GetUnit("IntakeUnit")}";

            var showRpf = Model.DetailedIndividualDrillDownRecords
                .Any(r => !double.IsNaN(r.Rpf) && r.Rpf != 1d);

            var hiddenPropertiesDetailed = new List<string>();

            if (!showRpf) {
                hiddenPropertiesDetailed.Add("Rpf");
                hiddenPropertiesDetailed.Add("EquivalentExposure");
                hiddenPropertiesDetailed.Add("Percentage");
            }
            if (!Model.IsProcessing) {
                hiddenPropertiesDetailed.Add("ProcessingFactor");
                hiddenPropertiesDetailed.Add("ProcessingCorrectionFactor");
                hiddenPropertiesDetailed.Add("ProcessingTypeDescription");
            }
            sb.AppendTable(
                Model,
                Model.DetailedIndividualDrillDownRecords,
                $"DietaryIndividualIntakeDrillDownTable-{Model.DrilldownIndex}",
                ViewBag,
                caption: descriptionIndividual,
                saveCsv: true,
                header: true,
                displayLimit: 20,
                hiddenProperties: hiddenPropertiesDetailed
            );
        }
    }
}
