using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ConcentrationCorrelationsChartCreatorBase : OxyPlotChartCreator {

        public ConcentrationCorrelationsChartCreatorBase(
            int width,
            int height
        ) : base() {
            Width = width;
            Height = height;
        }

        protected ScatterSeries createScatterMask(OxyColor color) {
            return new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromArgb(125, color.R, color.G, color.B),
                MarkerSize = 3,
                MarkerStroke = color,
                MarkerStrokeThickness = 0.4,
            };
        }

        protected CustomScatterErrorSeries createCustomScatterErrorSeries() {
            return new CustomScatterErrorSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.White,
                MarkerSize = 3,
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 0.4,
                ErrorBarColor = OxyColor.FromArgb(125, 0, 0, 0),
                ErrorBarStrokeThickness = 1,
                ErrorBarStopWidth = 0,
            };
        }
    }
}
