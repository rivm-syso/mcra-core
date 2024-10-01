using MCRA.General;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.DoseResponseModels, typeof(RawDoseResponseModel))]
    [RawTableObjectType(RawDataSourceTableID.DoseResponseModelBenchmarkDoses, typeof(RawDoseResponseModelBenchmarkDose))]
    [RawTableObjectType(RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain, typeof(RawDoseResponseModelBenchmarkDoseUncertain))]
    public sealed class RawDoseResponseModelData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.DoseResponseModels;

        public override ActionType ActionType => ActionType.DoseResponseModels;

        public List<RawDoseResponseModel> DoseResponseModels { get; private set; }
        public List<RawDoseResponseModelBenchmarkDose> BenchmarkDoses { get; private set; }
        public List<RawDoseResponseModelBenchmarkDoseUncertain> BenchmarkDosesUncertain { get; private set; }

        public RawDoseResponseModelData() : base() {
            DoseResponseModels = [];
            BenchmarkDoses = [];
            BenchmarkDosesUncertain = [];
            DataTables.Add(RawDataSourceTableID.DoseResponseModels, new GenericRawDataTable<RawDoseResponseModel>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModels,
                Records = DoseResponseModels
            });
            DataTables.Add(RawDataSourceTableID.DoseResponseModelBenchmarkDoses, new GenericRawDataTable<RawDoseResponseModelBenchmarkDose>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModelBenchmarkDoses,
                Records = BenchmarkDoses
            });
            DataTables.Add(RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain, new GenericRawDataTable<RawDoseResponseModelBenchmarkDoseUncertain>() {
                RawDataSourceTableID = RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain,
                Records = BenchmarkDosesUncertain
            });
        }
    }
}
