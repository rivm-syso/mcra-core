using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IntakePercentageSectionView : SectionView<IntakePercentageSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var intakePercentageRecords = Model.IntakePercentageRecords;
            bool showUncertainty = intakePercentageRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);
            var hiddenProperties = new List<string>();
            if (!showUncertainty) {
                hiddenProperties.Add("LowerBoundPercentage");
                hiddenProperties.Add("UpperBoundPercentage");
                hiddenProperties.Add("LowerBoundNumberOfPeopleExceedanceExposureLevel");
                hiddenProperties.Add("UpperBoundNumberOfPeopleExceedanceExposureLevel");
            }
            //render HTML
            var description = string.Empty;
            if (Model.ReferenceSubstance != null) {
                description = $"Reference: {Model.ReferenceSubstance.Name}, {Model.ReferenceSubstance.Code}";
            }
            sb.AppendDescriptionParagraph(description);
            sb.AppendTable(
                Model,
                intakePercentageRecords,
                "IntakePercentageTable",
                ViewBag,
                caption: "Exposure percentages.",
                saveCsv: true,
                header: true,
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

                var chartCreator = new IntakePercentageChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "IntakePercentageChart",
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
