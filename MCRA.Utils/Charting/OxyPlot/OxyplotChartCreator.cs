using System.Diagnostics;
using OxyPlot;
using OxyPlot.Core.Drawing;
using SvgExporter = OxyPlot.SvgExporter;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotChartCreator : IChartCreator {

        private int _width = 600;
        private int _height = 400;

        /// <summary>
        /// Unique identifier of the chart.
        /// </summary>
        public abstract string ChartId { get; }

        /// <summary>
        /// Chart title.
        /// </summary>
        public virtual string Title { get; }

        /// <summary>
        /// Width of the chart.
        /// </summary>
        public virtual int Width {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Height of the chart.
        /// </summary>
        public virtual int Height {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Abstract method that should implement creation of the plot model.
        /// </summary>
        /// <returns></returns>
        public abstract PlotModel Create();

        public string ToSvgString(int width, int height) {
            var plotModel = Create();
            plotModel = setFonts(plotModel);
            var str = SvgExporter.ExportToString(plotModel, width, height, false);
            // Note: Hashes (#) are not correctly encoded, so replace "#" by "%23"
            // (JS equivalent: encodeURIComponent).
            str = str.Replace("#", "%23");
            return str;
        }

        public void CreateToSvg(string fileName) {
            using (var stream = File.Create(fileName)) {
                var plotModel = Create();
                plotModel = setFonts(plotModel);
                SvgExporter.Export(plotModel, stream, Width, Height, true);
            }
        }

        public void CreateToPng(string fileName) {
            var plotModel = Create();
            plotModel.Background = OxyColors.White;
            const int scale = 1;
            if (!File.Exists(fileName) || Debugger.IsAttached) {
                PngExporter.Export(plotModel, fileName, scale * Width, scale * Height, scale * 96);
            }
        }

        public void WritePngToStream(Stream stream) {
            var plotModel = Create();
            plotModel.Background = OxyColors.White;
            const int scale = 1;
            if (stream != null || Debugger.IsAttached) {
                var exporter = new PngExporter {
                    Height = scale * Height,
                    Width = scale * Width,
                    Resolution = scale * 96
                };
                exporter.Export(plotModel, stream);
            }
        }

        protected PlotModel createDefaultPlotModel(string title) {
            return new PlotModel() {
                TitleFontSize = 13,
                TitleFontWeight = FontWeights.Bold,
                IsLegendVisible = false,
                Title = title,
                ClipTitle = false
            };
        }

        protected PlotModel createDefaultPlotModel() {
            return new PlotModel() {
                TitleFontSize = 13,
                TitleFontWeight = FontWeights.Bold,
                IsLegendVisible = false,
            };
        }

        private PlotModel setFonts(PlotModel plotModel) {
            var font = "Calibri";
            var fontWeights = FontWeights.Bold;

            plotModel.DefaultFont = font;
            plotModel.TitleFont = font;
            plotModel.TitleFontWeight = fontWeights;
            plotModel.TitleFontSize = 13;
            plotModel.Padding = new OxyThickness(20, 10, 0, 10);

            foreach (var Legend in plotModel.Legends) {
                Legend.LegendFont = font;
                Legend.LegendFontWeight = fontWeights;
                Legend.LegendFontSize = 12;
                Legend.LegendTitleFontWeight = fontWeights;
            }

            foreach (var axis in plotModel.Axes) {
                axis.Font = font;
                axis.FontWeight = fontWeights;
                axis.FontSize = 12;
                axis.TitleFontWeight = fontWeights;
                axis.TitleFont = font;
                axis.TitleFontSize = 15;
                axis.AxisTitleDistance = 10;
            }

            foreach (var serie in plotModel.Series) {
                serie.Font = font;
                serie.FontWeight = fontWeights;
                serie.FontSize = 12;
            }

            foreach (var annotation in plotModel.Annotations) {
                annotation.Font = font;
                annotation.FontWeight = fontWeights;
                annotation.FontSize = 13;
            }
            return plotModel;
        }
    }
}
