using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleHazardIndexSectionView : SectionView<SingleHazardIndexSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.HazardIndexRecords
                .Any(c => c.HazardIndexPercentiles[0].UncertainValues?.Any() ?? false);

            // Section description
            var substancesString = Model.OnlyCumulativeOutput ? $" for cumulative substance" : string.Empty;
            var effectString = !string.IsNullOrEmpty(Model.EffectName) ? Model.EffectName : "based on multiple effects";
            var descriptionString = $"Hazard index{substancesString} for {effectString}.";

            // Table
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerHI_UncLower");
                hiddenProperties.Add("PUpperHI_UncUpper");
                hiddenProperties.Add("PLowerHIUncP50");
                hiddenProperties.Add("HIP50UncP50");
                hiddenProperties.Add("PUpperHIUncP50");
                hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
            } else {
                hiddenProperties.Add("PLowerHINom");
                hiddenProperties.Add("HIP50Nom");
                hiddenProperties.Add("PUpperHINom");
                hiddenProperties.Add("ProbabilityOfCriticalEffect");
            }
            var records = (Model.HazardIndexRecords.Any(c => c.HIP50UncP50 > 0))
             ? Model.HazardIndexRecords.OrderByDescending(c => c.PUpperHI_UncUpper).ToList()
             : Model.HazardIndexRecords.OrderByDescending(c => c.PUpperHINom).ToList();
            sb.AppendTable(
               Model,
               records,
               "SingleHazardIndexTable",
               ViewBag,
               caption: descriptionString,
               saveCsv: true,
               sortable: false,
               rotate: true,
               hiddenProperties: hiddenProperties
           );

            // Figure
            var caption = $"Safety chart: bar shows variability of HI (range {pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }
            sb.AppendChart(
                name: "HazardIndexBySubstanceChart",
                chartCreator: new SingleHazardIndexHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                fileType: ChartFileType.Png,
                section: Model,
                viewBag: ViewBag,
                caption: caption,
                saveChartFile: true
            );
        }
    }
}
