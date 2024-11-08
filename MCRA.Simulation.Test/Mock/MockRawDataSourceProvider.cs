using MCRA.Data.Raw;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock {
    class MockRawDataSourceProvider: IRawDataProvider {
        private Func<IRawDataManager> _rawDataManagerFactory;

        public MockRawDataSourceProvider(Func<IRawDataManager> rawDataManagerFactory) {
            _rawDataManagerFactory = rawDataManagerFactory;
        }

        public IRawDataManager CreateRawDataManager() {
            return _rawDataManagerFactory.Invoke();
        }

        public ICollection<int> GetRawDatasourceIds(SourceTableGroup tableGroup) {
            return [(int)tableGroup];
        }

        public HashSet<string> GetFilterCodes(ScopingType scopingType) {
            return null;
        }

        public bool HasKeysFilter(ScopingType scopingType) {
            return GetFilterCodes(scopingType)?.Count > 0;
        }
    }
}
