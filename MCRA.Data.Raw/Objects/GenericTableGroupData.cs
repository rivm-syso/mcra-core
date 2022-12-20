using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MCRA.Data.Raw.Objects {

    [KnownType("RawDoseResponseModelData")]
    [KnownType("RawDietaryExposuresData")]
    [KnownType("RawAssessmentGroupMembershipsData")]
    [KnownType("RawRelativePotencyFactorsData")]
    [KnownType("RawHumanMonitoringData")]
    public abstract class GenericTableGroupData : IRawTableGroupData {

        public abstract SourceTableGroup SourceTableGroup { get; }

        public IDictionary<RawDataSourceTableID, IRawDataTable> DataTables { get; set; }

        public GenericTableGroupData() {
            DataTables = new Dictionary<RawDataSourceTableID, IRawDataTable>();
        }

        public virtual Dictionary<RawDataSourceTableID, Type> RawTableRecordObjectTypes() {
            var result = GetType()
                .GetCustomAttributes(typeof(RawTableObjectTypeAttribute), false)
                .Cast<RawTableObjectTypeAttribute>()
                .ToDictionary(r => r.TableId, r => r.Type);
            return result;
        }
    }
}
