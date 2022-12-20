using MCRA.Data.Compiled;
using MCRA.Data.Raw;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public abstract class RawTableGroupDataConverterBase<T> where T : IRawTableGroupData {
        public abstract T FromCompiledData(CompiledData data);
    }
}
