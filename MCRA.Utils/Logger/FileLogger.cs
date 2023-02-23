using System.Text;

namespace MCRA.Utils.Logger {
    public sealed class FileLogger : ILogger, IDisposable {

        private readonly string _logFile;
        private readonly StringBuilder _stringBuilder;

        public FileLogger(string filename = null) {
            if (string.IsNullOrEmpty(filename)) {
                _logFile = Path.Combine(Path.GetTempPath(), "log.txt");
            } else {
                _logFile = filename;
            }
            _stringBuilder = new StringBuilder();
        }

        public void Log(string message) {
            _stringBuilder.AppendLine(message);
        }

        public string Print() {
            return _stringBuilder.ToString();
        }

        public void Write() {
            File.WriteAllText(_logFile, _stringBuilder.ToString());
        }

        public void Reset() {
            _stringBuilder.Clear();
        }

        #region IDisposable Members

        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing) {
            File.WriteAllText(_logFile, _stringBuilder.ToString());
        }

        #endregion
    }
}
