using MCRA.General;

namespace MCRA.Data.Raw {
    public interface IRawDataProvider {
        IRawDataManager CreateRawDataManager();
        ICollection<int> GetRawDatasourceIds(SourceTableGroup tableGroup);

        HashSet<string> GetFilterCodes(ScopingType scopingType);
        bool HasKeysFilter(ScopingType scopingType);
    }
}
