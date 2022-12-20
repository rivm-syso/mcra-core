using MCRA.Data.Raw;

namespace MCRA.Data.Management.RawDataManagers {
    public class BinaryRawDataManagerFactory : IRawDataManagerFactory {
        private BinaryRawDataManager _manager;
        public BinaryRawDataManagerFactory(string folderName) {
            _manager = new BinaryRawDataManager(folderName);
        }
        public IRawDataManager CreateRawDataManager() {
            return _manager;
        }
    }
}
