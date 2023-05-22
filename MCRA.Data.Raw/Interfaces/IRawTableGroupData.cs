using MCRA.General;

namespace MCRA.Data.Raw {
    public interface IRawTableGroupData {
        SourceTableGroup SourceTableGroup { get; }
        ActionType ActionType { get; }
        Dictionary<RawDataSourceTableID, Type> RawTableRecordObjectTypes();
        IDictionary<RawDataSourceTableID, IRawDataTable> DataTables { get; set; }
    }
}
