using MCRA.General;
using System;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Objects.DoseResponseModels {

    [RawTableObjectType(RawDataSourceTableID.DoseResponseModels, typeof(RawDoseResponseModelRecord))]
    [RawTableObjectType(RawDataSourceTableID.DoseResponseModelBenchmarkDoses, typeof(RawDoseResponseModelBenchmarkDoseRecord))]
    [RawTableObjectType(RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain, typeof(RawDoseResponseModelBenchmarkDoseUncertainRecord))]
    public sealed class RawDoseResponseModelData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.DoseResponseModels;

        public List<RawDoseResponseModelRecord> DoseResponseModels { get; private set; }
        public List<RawDoseResponseModelBenchmarkDoseRecord> BenchmarkDoses { get; private set; }
        public List<RawDoseResponseModelBenchmarkDoseUncertainRecord> BenchmarkDosesUncertain { get; private set; }

        public RawDoseResponseModelData() : base() {
            DoseResponseModels = new List<RawDoseResponseModelRecord>();
            BenchmarkDoses = new List<RawDoseResponseModelBenchmarkDoseRecord>();
            BenchmarkDosesUncertain = new List<RawDoseResponseModelBenchmarkDoseUncertainRecord>();
            DataTables.Add(RawDataSourceTableID.DoseResponseModels, new GenericRawDataTable<RawDoseResponseModelRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModels,
                Records = DoseResponseModels
            });
            DataTables.Add(RawDataSourceTableID.DoseResponseModelBenchmarkDoses, new GenericRawDataTable<RawDoseResponseModelBenchmarkDoseRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModelBenchmarkDoses,
                Records = BenchmarkDoses
            });
            DataTables.Add(RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain, new GenericRawDataTable<RawDoseResponseModelBenchmarkDoseUncertainRecord>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain,
                Records = BenchmarkDosesUncertain
            });
        }
    }
}
