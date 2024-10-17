using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class IndividualContributionsUpperBySubstanceBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private readonly bool _showOutliers;
        protected readonly ContributionsForIndividualsUpperSection _section;

        public IndividualContributionsUpperBySubstanceBoxPlotChartCreator(
            ContributionsForIndividualsUpperSection section,
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
                var pictureId = "cc683497-3a9f-4883-8ef3-b4c3b43149a5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var percentageAtRisk = _section.PercentagesAtRisk.Percentages.Any()
                    ? _section.PercentagesAtRisk.MedianContribution
                    : _section.PercentagesAtRisk.Percentage;
                var description = $"Boxplots of individual contributions by substance to the " +
                    $"upper {percentageAtRisk:F1}% of the distribution";
                if (_section.HbmBoxPlotRecords.Count == 1) {
                    description += $" (n={_section.HbmBoxPlotRecords.First().NumberOfPositives})";
                }
                description += ".";
                if (_showOutliers) {
                    description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, " +
                        "and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                } else {
                    description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95.";
                }
                return description;
            }
        }

        public override PlotModel Create() {
            return create(
                _section.HbmBoxPlotRecords,
                "Contribution risk (%)",
                _showOutliers,
                true
            );
        }
    }
}
