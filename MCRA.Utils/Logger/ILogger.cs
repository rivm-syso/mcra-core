namespace MCRA.Utils.Logger {
    public interface ILogger {
        void Log(string message);
        string Print();
        void Reset();
    }
}
