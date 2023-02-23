using MCRA.Data.Raw;
using MCRA.General;

namespace MCRA.Data.Management.RawDataProviders {
    public class SimpleRawDataProvider : IRawDataProvider {

        private Func<IRawDataManager> _rawDataManagerFactory;

        public SimpleRawDataProvider(Func<IRawDataManager> rawDataManagerFactory) {
            _rawDataManagerFactory = rawDataManagerFactory;
        }

        public IRawDataManager CreateRawDataManager() {
            if (_rawDataManagerFactory == null) {
                return null;
            }
            return _rawDataManagerFactory.Invoke();
        }

        public ICollection<int> GetRawDatasourceIds(SourceTableGroup tableGroup) {
            return new List<int>() { 1 };
        }

        public HashSet<string> GetFilterCodes(ScopingType scopingType) {
            return null;
        }

        public bool HasKeysFilter(ScopingType scopingType) {
            return GetFilterCodes(scopingType)?.Any() ?? false;
        }
    }
}
