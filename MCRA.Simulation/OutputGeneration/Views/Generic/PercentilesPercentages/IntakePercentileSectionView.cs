using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IntakePercentileSectionView : SectionView<IntakePercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var intakePercentileRecords = Model.IntakePercentileRecords;
            bool showUncertainty = intakePercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var uncertaintyMeanOfExposure = string.Empty;
            var hiddenProperties = new List<string>();
            if (!showUncertainty) {
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
                hiddenProperties.Add("Median");
            } else if (Model.MeanOfExposure.Any()) {
                uncertaintyMeanOfExposure = $"({Model.MeanOfExposure.First().Percentile(Model.UncertaintyLowerLimit).ToString("G4")}, {Model.MeanOfExposure.First().Percentile(Model.UncertaintyUpperLimit).ToString("G4")})";
            }

            //render HTML
            var description = string.Empty;
            if (Model.ReferenceSubstance != null) {
                description = $"Reference: {Model.ReferenceSubstance.Name} {Model.ReferenceSubstance.Code}.";
            }
            if (Model.MeanOfExposure.Any()) {
                description = description + $" Mean exposure: {Model.MeanOfExposure.ReferenceValues.First():G3} {uncertaintyMeanOfExposure} ({ViewBag.GetUnit("IntakeUnit")}).";
            }
            sb.AppendDescriptionParagraph(description);

            sb.AppendTable(
                Model,
                intakePercentileRecords,
                "IntakePercentileTable",
                ViewBag,
                caption: "Exposure percentiles.",
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
                var bootstrapResultsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "IntakePercentileBootstrapTable", Model, Model.GetIntakePercentileBootstrapRecords(),
                    ViewBag, true, hiddenProperties
                );

                var chartCreator = new IntakePercentileChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "IntakePercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true,
                    null,
                    bootstrapResultsDataSection
                );

                sb.AppendDescriptionParagraph($"The boxplots for uncertainty show the p{lowerBoxDefault} and p{upperBoxDefault} as edges of the box, " +
                    $"and p{lowerBound} and p{upperBound} as edges of the whiskers. The reference value is indicated with the dashed black line, the median " +
                    $"with the solid black line within the box. Outliers are displayed as dots outside the wiskers.");
            }
        }
    }
}
