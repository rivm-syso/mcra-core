using MCRA.Data.Raw.Objects.RawObjects;
using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableGroups {

    [RawTableObjectType(RawDataSourceTableID.AnalyticalMethods, typeof(RawAnalyticalMethod))]
    [RawTableObjectType(RawDataSourceTableID.AnalyticalMethodCompounds, typeof(RawAnalyticalMethodCompound))]
    [RawTableObjectType(RawDataSourceTableID.HumanMonitoringSamples, typeof(RawHumanMonitoringSample))]
    [RawTableObjectType(RawDataSourceTableID.HumanMonitoringSampleAnalyses, typeof(RawHumanMonitoringSampleAnalysis))]
    [RawTableObjectType(RawDataSourceTableID.HumanMonitoringSampleConcentrations, typeof(RawHumanMonitoringSampleConcentration))]
    public sealed class RawHumanMonitoringData : GenericTableGroupData {

        public override SourceTableGroup SourceTableGroup => SourceTableGroup.HumanMonitoringData;

        public List<RawHumanMonitoringSurvey> HumanMonitoringSurveys { get; private set; }
        public List<RawAnalyticalMethod> AnalyticalMethods { get; private set; }
        public List<RawAnalyticalMethodCompound> AnalyticalMethodCompounds { get; private set; }
        public List<RawHumanMonitoringSample> HumanMonitoringSamples { get; private set; }
        public List<RawIndividual> Individuals { get; private set; }
        public List<RawHumanMonitoringSampleAnalysis> HumanMonitoringSampleAnalyses { get; private set; }
        public List<RawHumanMonitoringSampleConcentration> HumanMonitoringSampleConcentrations { get; private set; }

        public RawHumanMonitoringData() : base() {
            HumanMonitoringSurveys = new List<RawHumanMonitoringSurvey>();
            AnalyticalMethods = new List<RawAnalyticalMethod>();
            AnalyticalMethodCompounds = new List<RawAnalyticalMethodCompound>();
            Individuals = new List<RawIndividual>();
            HumanMonitoringSamples = new List<RawHumanMonitoringSample>();
            HumanMonitoringSampleAnalyses = new List<RawHumanMonitoringSampleAnalysis>();
            HumanMonitoringSampleConcentrations = new List<RawHumanMonitoringSampleConcentration>();
            DataTables.Add(RawDataSourceTableID.HumanMonitoringSurveys, new GenericRawDataTable<RawHumanMonitoringSurvey>() {
                RawDataSourceTableID = RawDataSourceTableID.HumanMonitoringSurveys,
                Records = HumanMonitoringSurveys
            });
            DataTables.Add(RawDataSourceTableID.AnalyticalMethods, new GenericRawDataTable<RawAnalyticalMethod>() {
                RawDataSourceTableID = RawDataSourceTableID.AnalyticalMethods,
                Records = AnalyticalMethods
            });
            DataTables.Add(RawDataSourceTableID.AnalyticalMethodCompounds, new GenericRawDataTable<RawAnalyticalMethodCompound>() {
                RawDataSourceTableID = RawDataSourceTableID.AnalyticalMethodCompounds,
                Records = AnalyticalMethodCompounds
            });
            DataTables.Add(RawDataSourceTableID.Individuals, new GenericRawDataTable<RawIndividual>() {
                RawDataSourceTableID = RawDataSourceTableID.Individuals,
                Records = Individuals
            });
            DataTables.Add(RawDataSourceTableID.HumanMonitoringSamples, new GenericRawDataTable<RawHumanMonitoringSample>() {
                RawDataSourceTableID = RawDataSourceTableID.HumanMonitoringSamples,
                Records = HumanMonitoringSamples
            });
            DataTables.Add(RawDataSourceTableID.HumanMonitoringSampleAnalyses, new GenericRawDataTable<RawHumanMonitoringSampleAnalysis>() {
                RawDataSourceTableID = RawDataSourceTableID.HumanMonitoringSampleAnalyses,
                Records = HumanMonitoringSampleAnalyses
            });
            DataTables.Add(RawDataSourceTableID.HumanMonitoringSampleConcentrations, new GenericRawDataTable<RawHumanMonitoringSampleConcentration>() {
                RawDataSourceTableID = RawDataSourceTableID.HumanMonitoringSampleConcentrations,
                Records = HumanMonitoringSampleConcentrations
            });
        }
    }
}
