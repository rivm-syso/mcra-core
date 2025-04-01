using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryExposureBySubstanceBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly DietaryExposuresBySubstanceSection _section;
        private readonly bool _showOutliers;

        public DietaryExposureBySubstanceBoxPlotChartCreator(DietaryExposuresBySubstanceSection section) {
            _section = section;
            _showOutliers = false;
            Width = 500;
            Height = 80 + Math.Max(_section.SubstanceBoxPlotRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Green;
            StrokeColor = OxyColors.DarkGreen;
        }

        public override string ChartId {
            get {
                var pictureId = "7fadbced-d6a2-4e9b-a64a-e59724bddea5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override PlotModel Create() {
            return create(
                _section.SubstanceBoxPlotRecords.Cast<BoxPlotChartRecord>().ToList(),
                $"Exposure ({_section.ExposureUnit.GetShortDisplayName()})",
                _showOutliers
            );
        }
    }
}
