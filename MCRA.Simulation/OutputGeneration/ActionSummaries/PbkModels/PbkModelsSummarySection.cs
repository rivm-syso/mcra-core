using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelsSummarySection : SummarySection {

        public List<PbkModelInstanceSummaryRecord> Records { get; set; }

        /// <summary>
        /// Summarize kinetic model instances
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void Summarize(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            Records = [];
            var humanModels = kineticModelInstances.Where(r => r.IsHumanModel).ToList();
            foreach (var model in humanModels) {
                var inputSubstances = model.KineticModelSubstances?
                    .Where(r => r.SubstanceDefinition?.IsInput ?? false)
                    .Select(r => r.Substance)
                    .ToList() ?? model.Substances;
                var record = new PbkModelInstanceSummaryRecord() {
                    PbkModelInstanceName = model.Name,
                    PbkModelInstanceCode = model.IdModelInstance,
                    PbkModelDefinitionCode = model.KineticModelDefinition.Id,
                    PbkModelDefinitionName = model.KineticModelDefinition.Name,
                    SubstanceCodes = model.Substances.Any() 
                        ? string.Join(", ", model.Substances.Select(c => c.Code)) : string.Empty,
                    SubstanceNames = model.Substances.Any() 
                        ? string.Join(", ", model.Substances.Select(c => c.Name)) : string.Empty,
                    Species = "Human",
                    InputSubstanceCodes = string.Join(",", inputSubstances.Select(c => c.Code)),
                    InputSubstanceNames = string.Join(",", inputSubstances.Select(c => c.Name))
                };
                Records.Add(record);
            }
        }
    }
}
