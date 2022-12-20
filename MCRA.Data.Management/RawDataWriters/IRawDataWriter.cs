using MCRA.Data.Raw;
using MCRA.General;

namespace MCRA.Data.Management.RawDataWriters {
    public interface IRawDataWriter {
        void Set<T>(T rawTableGroupData) where T : IRawTableGroupData;
        IRawTableGroupData Get(SourceTableGroup tableGroup);
        void Store();
    }
}
