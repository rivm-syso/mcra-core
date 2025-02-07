using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ChronicPerSubstanceDrilldownSectionView : SectionView<ChronicPerSubstanceDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenPropertiesSubstances = new List<string>();
            var showRpf = Model.IndividualSubstanceDrillDownRecords
                .Any(r => !double.IsNaN(r.Rpf) && r.Rpf != 1d);

            var equivalents = Model.IsCumulative ? " equivalents" : "";

            var descriptionSubstance = $"Exposure {Model.ReferenceCompoundName}{equivalents} = exposure * relative potency factor, " +
                $"body weight: {Model.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}";

            var uniqueSubstanceNameCount = Model.IndividualSubstanceDrillDownRecords.Select(dd => dd.SubstanceName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (!showRpf) {
                hiddenPropertiesSubstances.Add("Rpf");
                hiddenPropertiesSubstances.Add("EquivalentExposure");
            }
            if (uniqueSubstanceNameCount > 1) {
                var chartCreator = new DietaryChronicCompoundPieChartCreator(Model.IndividualSubstanceDrillDownRecords, Model.DrilldownIndex);
                sb.AppendChart(
                    $"DietaryChronicSubstancePieChart-{Model.DrilldownIndex}",
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
                Model.IndividualSubstanceDrillDownRecords,
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
