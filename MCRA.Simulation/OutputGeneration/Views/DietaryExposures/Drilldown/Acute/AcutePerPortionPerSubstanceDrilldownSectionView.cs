using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AcutePerPortionPerSubstanceDrilldownSectionView : SectionView<AcutePerPortionPerSubstanceDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.DrilldownRecord.DietaryExposure <= 0) {
                sb.AppendParagraph($"For individual: {Model.DrilldownRecord.IndividualId}, day {Model.DrilldownRecord.Day} no exposures available");
                return;
            }
            var hiddenPropertiesDetailed = new List<string>();
            var descriptionIndividual = $"Individual {Model.DrilldownRecord.IndividualId.ToHtml()}, day {Model.DrilldownRecord.Day.ToHtml()}, " +
                $"body weight: {Model.DrilldownRecord.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, " +
                $"sampling weight: {Model.DrilldownRecord.SamplingWeight:F2}";

            var showRpf = Model.DetailedIndividualDayDrillDownRecords.Any(c => !double.IsNaN(c.Rpf) && c.Rpf != 1D);
            if (Model.IsProcessing) {
                sb.AppendParagraph($"Exposure = portion amount * concentration in portion * processing factor / processing correction factor / {Model.DrilldownRecord.BodyWeight} (= body weight)");
            } else {
                sb.AppendParagraph($"Exposure = portion amount * concentration in portion  / {Model.DrilldownRecord.BodyWeight} (= body weight)");
            }
            if (showRpf) {
                sb.AppendParagraph($"{Model.ReferenceCompoundName.ToHtml()} equivalents exposure = RPF * exposure");
            }
            //if (item.OthersIntakePerMassUnit > 0) {
            //    sb.AppendParagraph("The summary below refers to the exposure due to riskdrivers");
            //}

            if (!Model.IsUnitVariability) {
                hiddenPropertiesDetailed.Add("UnitWeight");
                hiddenPropertiesDetailed.Add("UnitsInCompositeSample");
                hiddenPropertiesDetailed.Add("ConcentrationInSample");
                hiddenPropertiesDetailed.Add("VariabilityFactor");
                hiddenPropertiesDetailed.Add("StochasticVf");
            }
            if (!Model.IsProcessing) {
                hiddenPropertiesDetailed.Add("ProcessingFactor");
                hiddenPropertiesDetailed.Add("ProcessingCorrectionFactor");
            }
            if (!Model.IsUnitVariability && !Model.IsProcessing) {
                hiddenPropertiesDetailed.Add("ProcessingTypeDescription");
            }
            if (!showRpf) {
                hiddenPropertiesDetailed.Add("Rpf");
                hiddenPropertiesDetailed.Add("EquivalentExposure");
                hiddenPropertiesDetailed.Add("Percentage");
            }
            sb.AppendTable(
                Model,
                Model.DetailedIndividualDayDrillDownRecords,
                $"DietaryAcuteDrillDownDetailSectionTable-{Model.DrilldownIndex}",
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
