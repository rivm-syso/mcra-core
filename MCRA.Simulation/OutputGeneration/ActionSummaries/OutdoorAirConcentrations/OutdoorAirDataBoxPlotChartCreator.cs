using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OutdoorAirDataBoxPlotChartCreator : DataBoxPlotChartCreatorBase {

        private readonly OutdoorAirConcentrationsSummarySection _section;

        public OutdoorAirDataBoxPlotChartCreator(
            OutdoorAirConcentrationsSummarySection section
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
                var pictureId = "0d60caa5-8cdb-44c8-b1d7-ee53c025fd50";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of outdoor air substance concentration measurements.";
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


