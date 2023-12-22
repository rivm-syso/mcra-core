namespace MCRA.Utils.Charting {
    public interface IChartCreator {

        int Width { get; }

        int Height { get; }

        void CreateToPng(string filename);

        void CreateToSvg(string fileName);

        void WritePngToStream(Stream stream);

        string ToSvgString(int width, int height);

    }
}
