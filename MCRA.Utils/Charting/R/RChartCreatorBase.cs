namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class RChartCreatorBase : IChartCreator {

        public virtual int Width { get; set; } = 600;

        public virtual int Height { get; set; } = 400;

        public abstract string ToSvgString(int width, int height);

        public abstract void CreateToSvg(string fileName);

        public abstract void CreateToPng(string fileName);

        public abstract void WritePngToStream(Stream stream);
    }
}
