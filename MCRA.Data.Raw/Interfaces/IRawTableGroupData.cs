using MCRA.General;
using System;
using System.Collections.Generic;

namespace MCRA.Data.Raw {
    public interface IRawTableGroupData {
        SourceTableGroup SourceTableGroup { get; }
        Dictionary<RawDataSourceTableID, Type> RawTableRecordObjectTypes();
        IDictionary<RawDataSourceTableID, IRawDataTable> DataTables { get; set; }
    }
}
