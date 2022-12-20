using MCRA.Utils.DataFileReading;
using MCRA.General;
using MCRA.General.TableDefinitions;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Raw.Objects {
    public sealed class GenericRawDataTable<T> : IRawDataTable<T>
        where T : IRawDataTableRecord {

        public RawDataSourceTableID RawDataSourceTableID { get; set; }
        public List<T> Records { get; set; }

        public TableDefinition TableDefinition {
            get {
                return McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID);
            }
        }

        public string Id {
            get {
                return TableDefinition.Id;
            }
        }

        public List<IRawDataTableRecord> RecordsUntyped {
            get {
                return Records?.Cast<IRawDataTableRecord>().ToList();
            }
        }
    }
}
