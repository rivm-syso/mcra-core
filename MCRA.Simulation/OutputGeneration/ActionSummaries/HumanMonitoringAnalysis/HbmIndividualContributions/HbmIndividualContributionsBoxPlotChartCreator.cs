using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualContributionsBoxPlotChartCreator : HbmContributionsBoxPlotChartCreatorBase {

        private readonly HbmIndividualContributionsSection _section;
        private readonly bool _showOutliers;

        public HbmIndividualContributionsBoxPlotChartCreator(
            HbmIndividualContributionsSection section,
            bool showOutliers
        ) {
            _section = section;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Purple;
            StrokeColor = OxyColors.Purple;
        }

        public override string ChartId {
            get {
                var pictureId = "84f3e420-353a-46a9-86dc-ce0fb82420fe";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots contributions to cumulative concentrations for individuals.";
                if (_showOutliers) {
                    description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                } else {
                    description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95.";
                }
                return description;
            }
        }

        public override PlotModel Create() {
            return create(
                _section.HbmBoxPlotRecords, 
                "Contributions to cumulative concentration (%)", 
                _showOutliers, 
                true
            );
        }
    }
}
