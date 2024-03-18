using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualContributionsBySubstanceBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private readonly bool _showOutliers;
        private readonly ContributionsForIndividualsSection _section;

        public IndividualContributionsBySubstanceBoxPlotChartCreator(
            ContributionsForIndividualsSection section,
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
                var pictureId = "f40513c6-7e3a-46cd-903f-d2930c2dc8da";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of individual contributions by substance";
                if (_section.HbmBoxPlotRecords.Count == 1) {
                    description += $" (n={_section.HbmBoxPlotRecords.First().NumberOfPositives})";
                }
                description += ".";
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
                "Contribution risk (%)",
                _showOutliers,
                true
            );
        }
    }
}
