using System.IO;

namespace MCRA.Utils.Charting {
    public interface IChartCreator {

        string ChartId { get; }

        int Width { get; }

        int Height { get; }

        string Title { get; }

        void CreateToPng(string filename);

        void CreateToSvg(string fileName);

        void WritePngToStream(Stream stream);

        string ToSvgString(int width, int height);

    }
}
