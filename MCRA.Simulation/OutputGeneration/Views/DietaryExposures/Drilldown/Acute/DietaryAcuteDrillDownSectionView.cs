using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownSectionView : SectionView<DietaryAcuteDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenPropertiesOverall = new List<string>();
            var equivalents = Model.IsCumulative ? " equivalents" : "";

            //Render HTML
            var description = $"Drilldown of {Model.OverallIndividualDayDrillDownRecords.Count} individual days " +
                $"around {Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                $"of exposure distribution ({Model.ReferenceCompoundName.ToHtml()}{equivalents})";

            //DietaryAcuteDrillDownIndividualsSection
            sb.AppendTable(
                Model,
                Model.OverallIndividualDayDrillDownRecords,
                "DietaryAcuteDrillDownIndividualsSectionTable",
                ViewBag,
                caption: description,
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenPropertiesOverall
            );
        }
    }
}
