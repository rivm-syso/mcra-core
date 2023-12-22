namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class RChartCreatorBase : IChartCreator {

        private int _width = 600;
        private int _height = 400;

        public virtual int Width {
            get { return _width; }
            set { _width = value; }
        }

        public virtual int Height {
            get { return _height; }
            set { _height = value; }
        }

        public abstract string ToSvgString(int width, int height);

        public abstract void CreateToSvg(string fileName);

        public abstract void CreateToPng(string fileName);

        public abstract void WritePngToStream(Stream stream);
    }
}
