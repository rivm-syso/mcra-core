using MCRA.General;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Raw {
    public interface IRawTableDataReaderFactory {
        IDataSourceReader CreateProjectRawDataReader(int idProject);
        IDataSourceReader CreateDataSourceRawDataReader(int idDataSource);
        IDataSourceReader CreateDataSourceRawDataReader(DataSourceConfiguration dataSourceConfig);
    }
}
