using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Test.Mocks {

    public class MockProjectDataSourcesGenerator {

        public static IDictionary<SourceTableGroup, List<IRawDataSourceVersion>> FakeprojectDataSourceVersions(
            int id,
            params SourceTableGroup[] sourceTableGroups
        ) {
            var result = new Dictionary<SourceTableGroup, List<IRawDataSourceVersion>> {
                { SourceTableGroup.Unknown, new List<IRawDataSourceVersion>() {
                    new RawDataSourceVersionDto {
                    id = id,
                    Name = $"Mock data source {id}",
                    Checksum = $"M0CKCH3CK$UM-{id}",
                    TableGroups = sourceTableGroups.ToHashSet()
                }}}
            };
            return result;
        }
    }
}
