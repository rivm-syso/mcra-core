using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardPercentileSectionView : SectionView<HazardPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hazardPercentileRecords = Model.GetHazardPercentileRecords();
            bool showUncertainty = hazardPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var hiddenProperties = new List<string>();
            if (!showUncertainty) {
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
                hiddenProperties.Add("Median");
            } else {
                hiddenProperties.Add("ReferenceValue");
            }

            var uncertaintyMeanOfExposure = showUncertainty
                ? $" ({Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;

            sb.AppendParagraph($"Reference: {Model.Reference?.Name}.");
            sb.AppendParagraph($"Mean hazard characterisation: {Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfExposure} ({ViewBag.GetUnit("TargetDoseUnit")}).");

            sb.AppendTable(
                Model,
                hazardPercentileRecords,
                "HazardPercentileTable",
                ViewBag,
                caption: "Hazard characterisation percentiles.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

            if (showUncertainty) {
                var lowerBound = Model.UncertaintyLowerLimit;
                var upperBound = Model.UncertaintyUpperLimit;
                if (upperBound < lowerBound) {
                    var tmp = upperBound;
                    upperBound = lowerBound;
                    lowerBound = tmp;
                    Model.UncertaintyLowerLimit = lowerBound;
                    Model.UncertaintyUpperLimit = upperBound;
                }
                var upperBoxDefault = 75D;
                var lowerBoxDefault = 25D;
                if (upperBound < upperBoxDefault) {
                    upperBoxDefault = upperBound;
                }
                if (lowerBound > lowerBoxDefault) {
                    lowerBoxDefault = lowerBound;
                }
                var chartCreator = new HazardPercentileChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "HazardPercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.AppendDescriptionParagraph($"The boxplots for uncertainty show the p{lowerBoxDefault} and p{upperBoxDefault} as edges of the box, " +
                    $"and p{lowerBound} and p{upperBound} as edges of the whiskers. The reference value is indicated with the dashed black line, the median " +
                    $"with the solid black line within the box. Outliers are displayed as dots outside the wiskers.");
            }
        }
    }
}
