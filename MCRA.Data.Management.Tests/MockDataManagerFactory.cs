using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw;
using MCRA.General;
using System.Data;

namespace MCRA.Data.Management.Test {
    internal class MockDataManager : IRawDataManager {
        public bool CheckRawDataSourceAvailable(int idRawDataSource) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public (string Name, string Size) GetDatabaseInfo() => ("MockDatabase", "0");

        public IDictionary<string, string> GetTableInfo() => new Dictionary<string, string>();

        public HashSet<SourceTableGroup> LoadDataSourceFileIntoDb(IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
        }

        public HashSet<SourceTableGroup> LoadDataTablesIntoDb(DataTable[] dataTables, IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
        }

        public IDataReader OpenDataReader<T>(int idRawDataSource, out int[] fieldMap) where T : IConvertible {
            throw new NotImplementedException();
        }

        public IDataReader OpenDataReader(int idRawDataSource, RawDataSourceTableID idRawTable, out int[] fieldMap) {
            throw new NotImplementedException();
        }

        public IDataReader OpenKeysReader(int idRawDataSource, RawDataSourceTableID idRawTable, params (RawDataSourceTableID, string)[] linkedTables) {
            throw new NotImplementedException();
        }
    }

    internal class MockDataManagerFactory : IRawDataManagerFactory {
        public IRawDataManager CreateRawDataManager() {
            return new MockDataManager();
        }
    }
}