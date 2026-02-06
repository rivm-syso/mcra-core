using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceConcentrationsBoxPlotChartCreator : BoxPlotChartCreatorBase {

        protected SubstanceConcentrationsSummarySection _section;
        protected string _concentrationUnit;
        protected string _dataDescription;

        public SubstanceConcentrationsBoxPlotChartCreator(
            SubstanceConcentrationsSummarySection section,
            string dataDescription
        ) {
            _section = section;
            _dataDescription = dataDescription;
            Width = 500;
            Height = 80 + Math.Max(section.PercentileRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override PlotModel Create() {
            return create(
                _section.PercentileRecords.Cast<BoxPlotChartRecord>().ToList(),
                $"Concentration ({_section.UnitDisplayName})",
                true,
                true
            );
        }

        public override string ChartId {
            get {
                var pictureId = "8a033632-46ba-45a4-a836-ce288bbf87f6";
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
