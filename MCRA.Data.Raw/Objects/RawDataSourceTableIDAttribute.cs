using MCRA.General;
using System;
using System.Linq;

namespace MCRA.Data.Raw.Objects {

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RawDataSourceTableIDAttribute : Attribute {
        public RawDataSourceTableID TableId { get; set; }

        public RawDataSourceTableIDAttribute(RawDataSourceTableID tableId) {
            TableId = tableId;
        }

        public static RawDataSourceTableID GetRawDataSourceTableID(Type t) {
            var result = t.GetCustomAttributes(typeof(RawDataSourceTableIDAttribute), false).FirstOrDefault() as RawDataSourceTableIDAttribute;
            if (result != null) {
                return result.TableId;
            }
            throw new Exception("Undefined table ID");
        }
    }
}
