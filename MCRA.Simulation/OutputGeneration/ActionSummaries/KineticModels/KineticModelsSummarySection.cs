using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticModelsSummarySection : SummarySection {
        public List<KineticModelSummaryRecord> Records { get; set; }
        public List<KineticModelSubstanceRecord> SubstanceGroupRecords { get; set; }
        public List<AbsorptionFactorRecord> AbsorptionFactorRecords { get; set; }
        public List<ParameterRecord> ParameterSubstanceIndependentRecords { get; set; }
        public List<ParameterRecord> ParameterSubstanceDependentRecords { get; set; }

        /// <summary>
        /// Summarize kinetic model instances
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeHumanKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            Records = new List<KineticModelSummaryRecord>();
            SubstanceGroupRecords = new List<KineticModelSubstanceRecord>();
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
        /// Summarize absorption factors.
        /// </summary>
        public void SummarizeAbsorptionFactors(
            IDictionary<(ExposurePathType Route, Compound Substance), double> absorptionFactors,
            ICollection<KineticAbsorptionFactor> kineticAbsorptionFactors,
            ICollection<Compound> substances,
            bool aggregate
        ) {
            AbsorptionFactorRecords = new List<AbsorptionFactorRecord>();
            var defaults = new List<AbsorptionFactorRecord>();

            var potentialSubstanceRouteCombination = new Dictionary<(ExposurePathType Route, Compound Substance), bool>();
            foreach (var substance in substances) {
                potentialSubstanceRouteCombination[(ExposurePathType.Oral, substance)] = false;
                if (aggregate) {
                    potentialSubstanceRouteCombination[(ExposurePathType.Dermal, substance)] = false;
                    potentialSubstanceRouteCombination[(ExposurePathType.Inhalation, substance)] = false;
                }
            }

            foreach (var item in absorptionFactors) {
                var isSpecified = kineticAbsorptionFactors?
                    .Any(c => (c.ExposureRoute, c.Compound) == item.Key) ?? false;
                var record = new AbsorptionFactorRecord() {
                    CompoundCode = item.Key.Substance.Code,
                    CompoundName = item.Key.Substance.Name,
                    Route = item.Key.Route.ToString(),
                    AbsorptionFactor = item.Value,
                    IsDefault = isSpecified ? "data" : "default",
                };
                if (isSpecified && potentialSubstanceRouteCombination.TryGetValue(item.Key, out var present)) {
                    potentialSubstanceRouteCombination[item.Key] = true;
                    AbsorptionFactorRecords.Add(record);
                } else {
                    defaults.Add(record);
                }
            }
            var routes = potentialSubstanceRouteCombination
                .Where(c => !c.Value)
                .Select(c => c.Key.Route)
                .Distinct()
                .ToList();
            foreach (var route in routes) {
                var record = defaults.Single(c => c.Route.ToString() == route.ToString());
                AbsorptionFactorRecords.Add(record);
            }
        }

        /// <summary>
        /// Take only one substance, irrelevant which because physiological parameters
        /// are assumed independent of the substance.
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeParametersSubstanceIndependent(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            var humanModels = kineticModelInstances.Where(r => r.IsHumanModel).ToList();
            var substance = humanModels.SelectMany(c => c.Substances).First();
            ParameterSubstanceIndependentRecords = humanModels
                .Where(c => c.Substances.Contains(substance))
                .SelectMany(c => c.KineticModelDefinition.Parameters
                    .Where(i => !i.IsInternalParameter && i.Type == KineticModelParameterType.Physiological), (q, r) => new ParameterRecord() {
                        Parameter = r.Id,
                        Value = q.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter) ? parameter.Value : 0,
                        Unit = r.Unit,
                        Description = r.Description
                    }
                ).ToList();
        }

        /// <summary>
        /// Substance dependent parameters (metabolic)
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeParametersSubstanceDependent(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            var humanModels = kineticModelInstances.Where(r => r.IsHumanModel).ToList();
            ParameterSubstanceDependentRecords = humanModels
                .SelectMany(c => c.KineticModelDefinition.Parameters
                    .Where(i => !i.IsInternalParameter && i.Type != KineticModelParameterType.Physiological && !i.SubstanceParameterValues.Any()),
                        (q, r) => new ParameterRecord() {
                            Name = q.Substances.First().Name,
                            Code = q.Substances.First().Code,
                            Parameter = r.Id,
                            Unit = r.Unit,
                            Description = r.Description,
                            Value = q.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter) ? parameter.Value : 0

                        }
                ).ToList();

            var parameterRecords = humanModels
                .SelectMany(c => c.KineticModelDefinition.Parameters
                    .Where(i => !i.IsInternalParameter && i.Type != KineticModelParameterType.Physiological && i.SubstanceParameterValues.Any()),
                        (q, r) => {
                            var results = new List<ParameterRecord>();
                            var modelSubstances = q.KineticModelSubstances.Select(c => (
                                id: c.SubstanceDefinition.Id,
                                name: c.SubstanceDefinition.Name,
                                description: c.SubstanceDefinition.Description,
                                substance: c.Substance
                            )).ToDictionary(c => c.id);

                            foreach (var spv in r.SubstanceParameterValues) {
                                if (modelSubstances.TryGetValue(spv.IdSubstance, out var definition)) {
                                    results.Add(new ParameterRecord() {
                                        Name = definition.substance.Name,
                                        Code = definition.substance.Code,
                                        Parameter = spv.IdParameter,
                                        Unit = r.Unit,
                                        Description = $"{r.Description} {definition.description}",
                                        Value = q.KineticModelInstanceParameters.TryGetValue(spv.IdParameter, out var parameter) ? parameter.Value : 0
                                    });
                                }
                            }
                            return results;
                        }
                ).SelectMany(c => c).ToList();
            ParameterSubstanceDependentRecords.AddRange(parameterRecords);
        }

        /// <summary>
        /// Summarize animal kinetic models
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        public void SummarizeAnimalKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            Records = new List<KineticModelSummaryRecord>();
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
