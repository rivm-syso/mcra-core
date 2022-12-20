using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Management {
    public interface ICompiledLinkManager {
        HashSet<string> GetCodesInScope(ScopingType tableId);
        void LoadScope(SourceTableGroup tableGroup);
        Dictionary<ScopingType, DataReadingReport> GetDataReadingReports(SourceTableGroup tableGroup);
        IDictionary<string, StrongEntity> GetAllScopeEntities(ScopingType type);
        IDictionary<string, StrongEntity> GetAllEntities(ScopingType scopingType);
        IDictionary<string, StrongEntity> GetAllSourceEntities(
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
