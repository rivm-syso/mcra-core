using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelsSummarySection : SummarySection {

        public List<KineticModelSummaryRecord> Records { get; set; }
        public List<KineticModelSubstanceRecord> SubstanceGroupRecords { get; set; }

        /// <summary>
        /// Summarize kinetic model instances
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeHumanKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            Records = [];
            SubstanceGroupRecords = [];
            var humanModels = kineticModelInstances.Where(r => r.IsHumanModel).ToList();
            foreach (var model in humanModels) {
                var inputSubstances = model.KineticModelSubstances?
                    .Where(r => r.SubstanceDefinition?.IsInput ?? false)
                    .Select(r => r.Substance)
                    .ToList() ?? model.Substances;
                var record = new KineticModelSummaryRecord() {
                    KineticModelCode = model.KineticModelDefinition.Id,
                    KineticModelName = model.KineticModelDefinition.Name,
                    ModelInstanceName = model.Name,
                    ModelInstanceCode = model.IdModelInstance,
                    SubstanceCodes = model.Substances.Any() ? string.Join(", ", model.Substances.Select(c => c.Code)) : string.Empty,
                    SubstanceNames = model.Substances.Any() ? string.Join(", ", model.Substances.Select(c => c.Name)) : string.Empty,
                    Species = "Human",
                    InputSubstanceCodes = string.Join(",", inputSubstances.Select(c => c.Code)),
                    InputSubstanceNames = string.Join(",", inputSubstances.Select(c => c.Name))
                };
                Records.Add(record);
                if (model.HasMetabolites()) {
                    foreach (var item in model.KineticModelSubstances) {
                        var substanceGroupRecord = new KineticModelSubstanceRecord() {
                            KineticModelInstanceCode = model.IdModelInstance,
                            SubstanceName = item.Substance?.Name,
                            SubstanceCode = item.Substance?.Code,
                            KineticModelSubstanceCode = item.SubstanceDefinition.Id,
                            KineticModelSubstanceName = item.SubstanceDefinition.Name
                        };
                        SubstanceGroupRecords.Add(substanceGroupRecord);
                    }
                }
            }
        }

        /// <summary>
        /// Summarize animal kinetic models
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeAnimalKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            Records = [];
            var animalModels = kineticModelInstances.Where(r => !r.IsHumanModel).ToList();
            foreach (var model in animalModels) {
                var record = new KineticModelSummaryRecord() {
                    SubstanceCodes = model.Substances.Any() ? string.Join(",", model.Substances.Select(c => c.Code)) : string.Empty,
                    SubstanceNames = model.Substances.Any() ? string.Join(",", model.Substances.Select(c => c.Name)) : string.Empty,
                    Species = model.IdTestSystem,
                    ModelInstanceName = $"{model.IdModelDefinition}-{model.IdModelInstance}",
                };
                Records.Add(record);
            }
        }
    }
}
