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
                    .Except(instance.KineticModelDefinition.GetParameters().Select(r => r.Id));
                foreach (var parameter in unmatchedParameters) {
                    var record = new PbkModelParameterSummaryRecord() {
                        ModelInstanceCode = instance.IdModelInstance,
                        ModelInstanceName = instance.Name,
                        ParameterCode = parameter,
                        UnMatched = true
                    };
                    records.Add(record);
                }
                foreach (var parameter in instance.KineticModelDefinition.GetParameters()) {
                    if (parameter.IsInternalParameter) {
                        // Skip internal parameters
                        continue;
                    }
                    // Physiological parameter or physicochemical parameter not splitting out
                    var hasValue = instance.KineticModelInstanceParameters
                        .TryGetValue(parameter.Id, out var parameterValue);
                    var record = new PbkModelParameterSummaryRecord() {
                        ModelInstanceCode = instance.IdModelInstance,
                        ModelInstanceName = instance.Name,
                        ParameterCode = parameter.Id,
                        ParameterDescription = parameter.Description,
                        Missing = !hasValue,
                        Value = hasValue ? parameterValue.Value : parameter.DefaultValue,
                        Unit = parameter.Unit
                    };
                    records.Add(record);
                }
            }
            Records = records;
        }
    }
}
