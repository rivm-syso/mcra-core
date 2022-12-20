using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw {

    public interface IRawDataTable {
        string Id { get; }
        RawDataSourceTableID RawDataSourceTableID { get; set; }
        List<IRawDataTableRecord> RecordsUntyped { get; }
    }

    public interface IRawDataTable<T> : IRawDataTable
        where T : IRawDataTableRecord {
        List<T> Records { get; }
    }
}
