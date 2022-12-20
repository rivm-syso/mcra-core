using MCRA.Data.Raw;

namespace MCRA.Data.Management.RawDataManagers {
    public class CsvRawDataManagerFactory : IRawDataManagerFactory {
        private CsvRawDataManager _manager;
        public CsvRawDataManagerFactory(string folderName) {
            _manager = new CsvRawDataManager(folderName);
        }
        public IRawDataManager CreateRawDataManager() {
            return _manager;
        }
    }
}
