using MCRA.General.KineticModelDefinitions.SbmlPbkUtils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {
    public class SbmlToPbkModelDefinitionConverter {

        public KineticModelDefinition Convert(SbmlModel sbmlModel) {
            if (string.IsNullOrEmpty(sbmlModel.TimeUnits)) {
                throw new PbkModelException($"Error reading model [{sbmlModel.Id}]: model time unit not specified.");
            }
            if (!sbmlModel.UnitDefinitions.TryGetValue(sbmlModel.TimeUnits, out var timeUnits)) {
                throw new PbkModelException($"Error reading model [{sbmlModel.Id}]: model time unit not in unit definitions.");
            }
            if (sbmlModel.Compartments.Count == 0) {
                throw new PbkModelException($"Error reading model [{sbmlModel.Id}]: model does not contain any compartment.");
            }
            if (sbmlModel.Species.Count == 0) {
                throw new PbkModelException($"Error reading model [{sbmlModel.Id}]: model does not contain any species.");
            }
            var timeUnit = sbmlModel.GetModelTimeUnit();

            var result = new KineticModelDefinition {
                Id = sbmlModel.Id,
                Name = sbmlModel.Name ?? sbmlModel.Id,
                Format = KineticModelType.SBML,
                EvaluationFrequency = 1,
                Resolution = timeUnit,
                SbmlModel = sbmlModel
            };

            var inputCompartments = Enum.GetValues<ExposureRoute>()
                .Select(r => (
                    route: r,
                    compartment: sbmlModel.Compartments.Values
                        .Select(c => (compartment: c, priority: c.GetPriority(r)))
                        .Where(c => c.priority > 0)
                        .OrderByDescending(c => c.priority)
                        .Select(r => r.compartment)
                        .FirstOrDefault()
                ))
                .Where(r => r.compartment != null)
                .ToDictionary(r => r.compartment.Id);

            var inputSpecies = sbmlModel.Species.Values
                .Where(r => inputCompartments.ContainsKey(r.Compartment))
                .ToList();
            if (inputSpecies.Count == 0) {
                throw new PbkModelException($"Model {sbmlModel.Id} does not contain any chemical species.");
            }

            var unitsDictionary = sbmlModel.UnitDefinitions;
            result.Forcings = inputSpecies
                .Select(r => {
                    var amountUnit = unitsDictionary[r.SubstanceUnits].ToSubstanceAmountUnit();
                    var concentrationMassUnit = unitsDictionary[sbmlModel.Compartments[r.Compartment].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var route = sbmlModel.Compartments[r.Compartment].GetExposureRouteType();
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

            result.Outputs = sbmlModel.Species.Values
                .GroupBy(r => r.Compartment)
                .Select(r => {
                    var compartment = sbmlModel.Compartments[r.Key];
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

            result.Parameters = sbmlModel.Parameters.Values
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

            result.KineticModelSubstances = sbmlModel.Species.Values
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
