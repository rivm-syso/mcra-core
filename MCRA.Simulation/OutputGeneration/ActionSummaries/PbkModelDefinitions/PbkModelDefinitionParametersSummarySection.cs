using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelDefinitionParametersSummarySection : SummarySection {

        public string ModelCode { get; set; }
        public List<PbkModelDefinitionParameterSummaryRecord> Records { get; set; }

        public void Summarize(PbkModelDefinition modelDefinition) {
            var records = new List<PbkModelDefinitionParameterSummaryRecord>();
            foreach (var parameter in modelDefinition.KineticModelDefinition.GetParameters()) {
                // Substance dependent parameter splitting out over multiple substances
                var record = new PbkModelDefinitionParameterSummaryRecord() {
                    ParameterCode = parameter.Id,
                    ParameterName = parameter.Description,
                    Unit = parameter.Unit,
                    Value = parameter.DefaultValue,
                    Type = parameter.Type,
                };
                records.Add(record);
            }
            ModelCode = modelDefinition.IdModelDefinition;
            Records = records;
        }
    }
}
