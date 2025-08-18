using MCRA.Utils.Csv;

namespace MCRA.Simulation.OutputGeneration.Helpers {
    public static class CsvOutputWriterFactory {
        public static CsvWriterOptions DefaultOptions { get; } = new();
        public static CsvWriter Create() => new(DefaultOptions);
    }
}
