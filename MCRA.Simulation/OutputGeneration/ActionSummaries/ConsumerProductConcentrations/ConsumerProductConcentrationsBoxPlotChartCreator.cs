using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class ConsumerProductConcentrationsBoxPlotChartCreator : BoxPlotChartCreatorBase {

        protected ConsumerProductConcentrationsSection _section;
        protected string _dataDescription;
        protected string _substanceCode;
        private ConcentrationUnitSpec _unit;

        public ConsumerProductConcentrationsBoxPlotChartCreator(
            ConsumerProductConcentrationsSection section,
            string substanceCode,
            ConcentrationUnitSpec unit,
            string dataDescription
        ) {
            _section = section;
            _dataDescription = dataDescription;
            _substanceCode = substanceCode;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.ChartRecords[_unit].Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override PlotModel Create() {
            return create(
                [.. _section.ChartRecords[_unit]
                    .Where(c => c.SubstanceCode == _substanceCode)
                        .Cast<BoxPlotChartRecord>()],
                $"Concentration ({_unit.GetShortDisplayName()})",
                true,
                true
            );
        }

        public override string ChartId {
            get {
                var pictureId = "3e00427e-6bfe-4f59-8a20-d3d5863ae5e9";
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
