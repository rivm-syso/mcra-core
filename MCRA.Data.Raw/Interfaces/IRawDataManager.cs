using System.Data;
using MCRA.General;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw {
    public interface IRawDataManager : IDisposable {

        //T must be one of the RawDataSource enumeration types, IConvertible is used to (loosely) constrain to Enum types
        IDataReader OpenDataReader<T>(int idRawDataSource, out int[] fieldMap) where T : IConvertible;

        IDataReader OpenDataReader(
            int idRawDataSource,
            RawDataSourceTableID idRawTable,
            out int[] fieldMap
        );

        IDataReader OpenKeysReader(
            int idRawDataSource,
            RawDataSourceTableID idRawTable,
            params (RawDataSourceTableID TableId, string KeyField)[] linkedTables
        );

        /// <summary>
        /// Checks whether there is a data source with the specified id.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        bool CheckRawDataSourceAvailable(int idRawDataSource);

        /// <summary>
        /// Copies the data in the datasource file to the 'Raw' tables in the backend database.
        /// Executes only if the data has not already been loaded.
        /// </summary>
        /// <param name="rds"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        HashSet<SourceTableGroup> LoadDataSourceFileIntoDb(
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        );

        HashSet<SourceTableGroup> LoadDataTablesIntoDb(
            DataTable[] dataTables,
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        );

        (string Name, string Size) GetDatabaseInfo();

        IDictionary<string, string> GetTableInfo();
    }
}
