using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelParametersSummarySection : SummarySection {

        public List<PbkModelParameterSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            var records = new List<PbkModelParameterSummaryRecord>();
            foreach (var instance in kineticModelInstances) {
                foreach (var parameter in instance.KineticModelDefinition.Parameters) {
                    if (parameter.SubstanceParameterValues?.Any() ?? false) {
                        foreach (var substanceParameter in parameter.SubstanceParameterValues) {
                            // Substance dependent parameter splitting out over multiple substances
                            var record = new PbkModelParameterSummaryRecord() {
                                ModelInstanceCode = instance.IdModelInstance,
                                ModelInstanceName = instance.Name,
                                ParameterCode = substanceParameter.IdParameter,
                                Value = instance.KineticModelInstanceParameters
                                    .TryGetValue(parameter.Id, out var parameterValue)
                                        ? parameterValue.Value : double.NaN,
                                Unit = parameter.Unit,
                                ParameterName = parameter.Description
                            };
                            records.Add(record);
                        }
                    } else {
                        // Physiological parameter or physicochemical parameter not splitting out
                        var record = new PbkModelParameterSummaryRecord() {
                            ModelInstanceCode = instance.IdModelInstance,
                            ModelInstanceName = instance.Name,
                            ParameterCode = parameter.Id,
                            Value = instance.KineticModelInstanceParameters
                                .TryGetValue(parameter.Id, out var parameterValue) 
                                    ? parameterValue.Value : double.NaN,
                            Unit = parameter.Unit,
                            ParameterName = parameter.Description
                        };
                        records.Add(record);
                    }
                }
            }
            Records = records;
        }
    }
}
