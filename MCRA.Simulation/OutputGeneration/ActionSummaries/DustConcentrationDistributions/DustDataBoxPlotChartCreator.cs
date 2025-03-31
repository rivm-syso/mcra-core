using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustDataBoxPlotChartCreator : DataBoxPlotChartCreatorBase {

        private readonly DustConcentrationDistributionsSummarySection _section;

        public DustDataBoxPlotChartCreator(
            DustConcentrationDistributionsSummarySection section
        ) {
            _section = section;
            _concentrationUnit = _section.PercentileRecords.FirstOrDefault()?.Unit;
            Width = 500;
            Height = 80 + Math.Max(_section.PercentileRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "625956b0-4637-45b6-aa0a-9a6134b2d57f";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of dust substance concentration measurements.";
                description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                return description;
            }
        }

        public override PlotModel Create() {
            return create(
                _section.PercentileRecords,
                $"Concentration ({_concentrationUnit})"
            );
        }
    }
}


