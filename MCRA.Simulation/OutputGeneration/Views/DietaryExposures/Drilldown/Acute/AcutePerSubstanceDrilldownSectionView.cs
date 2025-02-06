using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AcutePerSubstanceDrilldownSectionView : SectionView<AcutePerSubstanceDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.DrilldownRecord.DietaryExposure <= 0) {
                sb.AppendParagraph($"For individual: {Model.DrilldownRecord.IndividualId}, day {Model.DrilldownRecord.Day} no exposures available");
                return;
            }

            var hiddenPropertiesSubstances = new List<string> {
                "Day"
            };
            var equivalents = Model.IsCumulative ? " equivalents" : "";
            if (!Model.IsCumulative) {
                hiddenPropertiesSubstances.Add("Rpf");
                hiddenPropertiesSubstances.Add("EquivalentExposure");
            }
            var substanceRecords = Model.IndividualSubstanceDrillDownRecords;
            var descriptionSubstance = $"Exposure {Model.ReferenceCompoundName}{equivalents} = exposure * relative potency factor" +
                $"body weight: {Model.DrilldownRecord.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}";
            if (substanceRecords.Count > 1 && substanceRecords.Count(c => c.ExposurePerDay > 0) > 1) {
                var chartCreator = new DietaryAcuteCompoundPieChartCreator(substanceRecords, Model.DrilldownIndex);
                sb.AppendChart(
                    $"DietaryAcuteSubstancePieChart-{Model.DrilldownIndex}",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendTable(
                Model,
                substanceRecords,
                $"DietaryAcuteSubstanceSectionTable-{Model.DrilldownIndex}",
                ViewBag,
                caption: descriptionSubstance,
                saveCsv: true,
                header: true,
                displayLimit: 20,
                hiddenProperties: hiddenPropertiesSubstances
            );
        }
    }
}
