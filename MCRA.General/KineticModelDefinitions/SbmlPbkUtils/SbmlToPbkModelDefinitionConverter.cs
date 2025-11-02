using MCRA.General.KineticModelDefinitions.SbmlPbkUtils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {
    public class SbmlToPbkModelDefinitionConverter {

        public KineticModelDefinition Convert(SbmlModel sbmlModel) {
            if (sbmlModel.TimeUnit == SbmlTimeUnit.NotSpecified) {
                throw new PbkModelException($"No time unit specified for model {sbmlModel.Id}.");
            }
            if (sbmlModel.Compartments.Count == 0) {
                throw new PbkModelException($"Model {sbmlModel.Id} does not contain any compartment.");
            }
            if (sbmlModel.Species.Count == 0) {
                throw new PbkModelException($"Model {sbmlModel.Id} does not contain any species.");
            }

            var result = new KineticModelDefinition {
                Id = sbmlModel.Id,
                Name = sbmlModel.Name ?? sbmlModel.Id,
                Format = KineticModelType.SBML,
                EvaluationFrequency = 1,
                Resolution = sbmlModel.TimeUnit.ToTimeUnit(),
                SbmlModel = sbmlModel
            };

            var inputCompartments = Enum.GetValues<ExposureRoute>()
                .Select(r => (
                    route: r,
                    compartment: sbmlModel.Compartments
                        .Select(c => (compartment: c, priority: c.GetPriority(r)))
                        .Where(c => c.priority > 0)
                        .OrderByDescending(c => c.priority)
                        .Select(r => r.compartment)
                        .FirstOrDefault()
                ))
                .Where(r => r.compartment != null)
                .ToDictionary(r => r.compartment.Id);

            var inputSpecies = sbmlModel.Species
                .Where(r => inputCompartments.ContainsKey(r.Compartment))
                .ToList();
            if (inputSpecies.Count == 0) {
                throw new PbkModelException($"Model {sbmlModel.Id} does not contain any chemical species.");
            }

            var unitsDictionary = sbmlModel.UnitDefinitions;
            var compartmentsLookup = sbmlModel.Compartments.ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
            result.Forcings = inputSpecies
                .Select(r => {
                    var amountUnit = unitsDictionary[r.SubstanceUnits].ToSubstanceAmountUnit();
                    var concentrationMassUnit = unitsDictionary[compartmentsLookup[r.Compartment].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var route = compartmentsLookup[r.Compartment].GetExposureRouteType();
                    var result = new KineticModelInputDefinition() {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Name,
                        Route = route,
                        Unit = doseUnit,
                    };
                    return result;
                })
                .ToList();

            var forcings = result.Forcings.GroupBy(c => c.Route);
            foreach (var forcing in forcings) {
                if (forcing.Count() > 1) {
                    throw new PbkModelException($"For route {forcing.Key.GetDisplayName()} multiple compartments are found.");
                };
            }

            result.Outputs = sbmlModel.Species
                .GroupBy(r => compartmentsLookup[r.Compartment].Id)
                .Select(r => {
                    var compartment = compartmentsLookup[r.Key];
                    var amountUnit = unitsDictionary[r.First().SubstanceUnits].ToSubstanceAmountUnit();
                    if (string.IsNullOrEmpty(compartment.Units)) {
                        throw new PbkModelException($"No unit specified for compartment [{r.Key}].");
                    }
                    var concentrationMassUnit = unitsDictionary[compartment.Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var result = new KineticModelOutputDefinition() {
                        Id = compartment.Id,
                        Description = compartment.Name,
                        Type = KineticModelOutputType.Concentration,
                        Unit = doseUnit,
                        Species = r
                            .Select(r => new KineticModelOutputSubstanceDefinition() {
                                IdSpecies = r.Id,
                                IdSubstance = r.GetSubstanceId()
                            })
                            .ToList(),
                        BiologicalMatrix = compartment.GetBiologicalMatrix()
                    };
                    return result;
                })
                .ToList();

            result.Parameters = sbmlModel.Parameters
                .Select((r, ix) => new KineticModelParameterDefinition() {
                    Id = r.Id,
                    IsInternalParameter = !r.IsConstant,
                    DefaultValue = r.DefaultValue,
                    Order = ix,
                    Description = r.Name,
                    Type = r.GetParameterType(),
                    Unit = !string.IsNullOrEmpty(r.Units)
                        && sbmlModel.UnitDefinitions.TryGetValue(r.Units, out var unitDef)
                        ? unitDef.GetUnitString() : null
                })
                .ToList();

            result.KineticModelSubstances = sbmlModel.Species
                .GroupBy(r => r.GetSubstanceId())
                .Select(g => new KineticModelSubstanceDefinition() {
                    Id = g.Key,
                    IsInput = g.Any(r => inputCompartments.ContainsKey(r.Compartment)),
                    Name = g.Key
                })
                .ToList();

            return result;
        }
    }
}
