using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryExposuresBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly NonDietaryExposuresSummarySection _section;
        private readonly string _dataDescription;

        public NonDietaryExposuresBoxPlotChartCreator(
            NonDietaryExposuresSummarySection section,
            string dataDescription
        ) {
            _section = section;
            _dataDescription = dataDescription;
            Width = 500;
            Height = 80 + Math.Max(section.PercentileRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.CornflowerBlue;
            StrokeColor = OxyColors.Blue;
        }

        public override PlotModel Create() {
            return create(
                _section.PercentileRecords.Cast<BoxPlotChartRecord>().ToList(),
                $"Exposure ({_section.ExternalExposureUnit.GetShortDisplayName()})",
                true,
                true
            );
        }

        public override string ChartId {
            get {
                var pictureId = "58b2dac2-8482-4b09-92f0-45b8ab1a9a67";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of {_dataDescription}."
                    + " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                return description;
            }
        }
    }
}
