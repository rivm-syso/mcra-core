using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryExposuresSummarySectionView : SectionView<NonDietaryExposuresSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (!Model.Records.Any()) {
                sb.AppendParagraph($"No {ExposureSource.OtherNonDiet.GetDisplayName()} exposures found/available for the specified scope.", "warning");
            } else {
                sb.AppendParagraph($"{ExposureSource.OtherNonDiet.GetDisplayName()} summary statistics per survey, route and substance");
                
                // Download table
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                   name: "NonDietaryExposuresPercentiles",
                   section: Model,
                   items: Model.PercentileRecords,
                   viewBag: ViewBag
               );

                var chartCreator = new NonDietaryExposuresBoxPlotChartCreator(Model, $"{ExposureSource.OtherNonDiet.GetDisplayName()} substances");
                sb.AppendChart(
                    "NonDietaryExposuresBoxPlotChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true,
                    chartData: percentileDataSection
                );

                sb.AppendTable(
                   Model,
                   Model.Records,
                   "NonDietaryExposuresSummaryTable",
                   ViewBag,
                   caption: $"{ExposureSource.OtherNonDiet.GetDisplayName()} exposures.",
                   saveCsv: true,
                   header: true
                );
                if (Model.NonDietarySurveyPropertyRecords.Any()) {
                    sb.AppendParagraph($"{ExposureSource.OtherNonDiet.GetDisplayName()} survey properties");
                    sb.AppendTable(
                       Model,
                       Model.NonDietarySurveyPropertyRecords,
                       "NonDietaryInputDataCovariatesTable",
                       ViewBag,
                       caption: $"{ExposureSource.OtherNonDiet.GetDisplayName()} input data covariates.",
                       saveCsv: true,
                       header: true
                    );
                }
                sb.AppendParagraph($"{ExposureSource.OtherNonDiet.GetDisplayName()} summary statistics per survey");
                sb.AppendTable(
                   Model,
                   Model.NonDietarySurveyProbabilityRecords,
                   "NonDietaryInputDataProbabilitiesTable",
                   ViewBag,
                   caption: $"{ExposureSource.OtherNonDiet.GetDisplayName()} input data probabilities.",
                   saveCsv: true,
                   header: true
                );
            }
        }
    }
}
