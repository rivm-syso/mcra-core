using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelDefinitionStateVariablesSummarySection : SummarySection {

        public string ModelCode { get; set; }
        public List<PbkModelDefinitionStateVariablesSummaryRecord> Records { get; set; }

        public void Summarize(PbkModelDefinition modelDefinition) {
            var records = new List<PbkModelDefinitionStateVariablesSummaryRecord>();
            foreach (var output in modelDefinition.KineticModelDefinition.Outputs) {
                var record = new PbkModelDefinitionStateVariablesSummaryRecord() {
                    CompartmentCode = output.Id,
                    CompartmentName = output.Description,
                    BiologicalMatrix = output.BiologicalMatrix != BiologicalMatrix.Undefined
                        ? output.BiologicalMatrix.ToString()
                        : null,
                };
                records.Add(record);
            }
            ModelCode = modelDefinition.IdModelDefinition;
            Records = records;
        }
    }
}
