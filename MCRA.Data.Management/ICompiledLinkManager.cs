using MCRA.Data.Compiled;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.General;

namespace MCRA.Data.Management {
    public interface ICompiledLinkManager {
        HashSet<string> GetCodesInScope(ScopingType tableId);
        void LoadScope(SourceTableGroup tableGroup);
        Dictionary<ScopingType, DataReadingReport> GetDataReadingReports(SourceTableGroup tableGroup);
        IDictionary<string, ScopeEntity> GetAllScopeEntities(ScopingType type);
        IDictionary<string, ScopeEntity> GetAllEntities(ScopingType scopingType);
        IDictionary<string, ScopeEntity> GetAllSourceEntities(
            ScopingType targetScope,
            ScopingType sourceScope = ScopingType.Unknown,
            int idDataSource = -1
        );
        HashSet<string> GetAllCodes(
            ScopingType targetScope,
            ScopingType sourceScope = ScopingType.Unknown,
            int idDataSource = -1
        );
    }
}
