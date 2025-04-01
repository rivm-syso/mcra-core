using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelParametersSummarySection : SummarySection {

        public List<PbkModelParameterSummaryRecord> Records { get; set; }

        public List<PbkModelParameterSummaryRecord> UnmatchedParameterRecords { get; set; }

        public void Summarize(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            var records = new List<PbkModelParameterSummaryRecord>();
            foreach (var instance in kineticModelInstances) {
                var unmatchedParameters = instance.KineticModelInstanceParameters
                    .Select(r => r.Key)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    .Except(instance.KineticModelDefinition.Parameters.Select(r => r.Id));
                foreach (var parameter in unmatchedParameters) {
                    var record = new PbkModelParameterSummaryRecord() {
                        ModelInstanceCode = instance.IdModelInstance,
                        ModelInstanceName = instance.Name,
                        ParameterCode = parameter,
                        UnMatched = true
                    };
                    records.Add(record);
                }
                foreach (var parameter in instance.KineticModelDefinition.Parameters) {
                    if (parameter.SubstanceParameterValues?.Count > 0) {
                        foreach (var substanceParameter in parameter.SubstanceParameterValues) {
                            // Substance dependent parameter splitting out over multiple substances
                            var hasValue = instance.KineticModelInstanceParameters
                                .TryGetValue(parameter.Id, out var parameterValue);
                            var record = new PbkModelParameterSummaryRecord() {
                                ModelInstanceCode = instance.IdModelInstance,
                                ModelInstanceName = instance.Name,
                                ParameterCode = substanceParameter.IdParameter,
                                Missing = !hasValue,
                                Value = hasValue ? parameterValue.Value : substanceParameter.DefaultValue,
                                Unit = parameter.Unit,
                                ParameterName = parameter.Description
                            };
                            records.Add(record);
                        }
                    } else {
                        // Physiological parameter or physicochemical parameter not splitting out
                        var hasValue = instance.KineticModelInstanceParameters
                            .TryGetValue(parameter.Id, out var parameterValue);
                        var record = new PbkModelParameterSummaryRecord() {
                            ModelInstanceCode = instance.IdModelInstance,
                            ModelInstanceName = instance.Name,
                            ParameterCode = parameter.Id,
                            Missing = !hasValue,
                            Value = hasValue ? parameterValue.Value : parameter.DefaultValue,
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
