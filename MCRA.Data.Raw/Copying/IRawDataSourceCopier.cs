using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying {
    public interface IRawDataSourceCopier {
        ICollection<SourceTableGroup> Copy(IDataSourceReader dataSourceReader, ProgressState progressState);
        void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState);
    }
}