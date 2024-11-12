using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionsSummarySection : SummarySection {

        public List<PbkModelDefinitionsRecord> Records { get; set; } = [];
        public void Summarize(ICollection<PbkModelDefinition> pbkModelDefinitions) {
            foreach (var definition in pbkModelDefinitions) {
                var record = new PbkModelDefinitionsRecord() {
                    Name = definition.Name,
                    Description = definition.Description,
                    FileName = Path.GetFileName(definition.FileName)
                };
                Records.Add(record);
            }
        }
    }
}
