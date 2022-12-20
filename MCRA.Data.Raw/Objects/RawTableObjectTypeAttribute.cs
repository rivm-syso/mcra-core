using MCRA.General;
using System;

namespace MCRA.Data.Raw.Objects {

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RawTableObjectTypeAttribute : Attribute {
        public RawDataSourceTableID TableId { get; set; }
        public Type Type { get; set; }

        public RawTableObjectTypeAttribute(RawDataSourceTableID tableId, Type type) {
            Type = type;
            TableId = tableId;
        }
    }
}
