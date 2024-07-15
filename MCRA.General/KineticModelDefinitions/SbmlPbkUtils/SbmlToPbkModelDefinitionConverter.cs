using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {
    public class SbmlToPbkModelDefinitionConverter {

        public KineticModelDefinition Convert(SbmlModel sbmlModel) {
            var result = new KineticModelDefinition();
            var unitsDictionary = sbmlModel.UnitDefinitions;
            var compartmentsLookup = sbmlModel.Compartments
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);

            result.Id = sbmlModel.Id;
            result.Name = sbmlModel.Name ?? sbmlModel.Id;
            result.Format = KineticModelType.SBML;
            result.EvaluationFrequency = 1;
            result.Resolution = sbmlModel.TimeUnit.ToString();

            var routes = new List<ExposureRoute>() {
                ExposureRoute.Oral,
                ExposureRoute.Dermal,
                ExposureRoute.Inhalation
            };
            var inputCompartments = routes
                .Select(r => (
                    route: r,
                    compartment: sbmlModel.Compartments
                        .Select(c => (
                            compartment: c,
                            priority: c.GetPriority(r)
                        ))
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

            result.Forcings = inputSpecies
                .Select(r => {
                    var amountUnit = unitsDictionary[r.SubstanceUnits].ToSubstanceAmountUnit();
                    var concentrationMassUnit = unitsDictionary[compartmentsLookup[r.Compartment].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var route = compartmentsLookup[r.Compartment].GetExposurePathType();
                    var result = new KineticModelInputDefinition() {
                        Id = r.Id,
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
                    throw new Exception($"For route {forcing.Key.GetDisplayName()} multiple compartments are found.");
                };
            }

            result.Outputs = sbmlModel.Species
                .GroupBy(r => compartmentsLookup[r.Compartment].Id)
                .Select(r => {
                    var amountUnit = unitsDictionary[r.First().SubstanceUnits].ToSubstanceAmountUnit();
                    if (string.IsNullOrEmpty(compartmentsLookup[r.Key].Units)) {
                        throw new Exception($"No unit specified for compartment [{r.Key}].");
                    }
                    var concentrationMassUnit = unitsDictionary[compartmentsLookup[r.Key].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var result = new KineticModelOutputDefinition() {
                        Id = compartmentsLookup[r.Key].Id,
                        Type = KineticModelOutputType.Concentration,
                        Unit = doseUnit,
                        Species = r
                            .Select(r => new KineticModelOutputSubstanceDefinition() {
                                IdSpecies = r.Id,
                                IdSubstance = r.GetSubstanceId()
                            })
                            .ToList(),
                        BiologicalMatrix = compartmentsLookup[r.Key].GetBiologicalMatrix()
                    };
                    return result;
                })
                .ToList();

            result.Parameters = sbmlModel.Parameters
                .Select((r, ix) => new KineticModelParameterDefinition() {
                    Id = r.Id,
                    IsInternalParameter = sbmlModel.AssignmentRules.Any(ar => ar.Variable == r.Id),
                    DefaultValue = r.DefaultValue,
                    Order = ix,
                    Description = r.Name,
                    Type = r.GetParameterType(),
                    Unit = (r.Units?.Equals("UNITLESS", StringComparison.OrdinalIgnoreCase) ?? true)
                        ? string.Empty : r.Units
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

            result.IdAgeParameter = sbmlModel.Parameters
                .FirstOrDefault(r => r.IsOfType(PbkModelParameterType.Age))?.Id;
            result.IdSexParameter = sbmlModel.Parameters
                .FirstOrDefault(r => r.IsOfType(PbkModelParameterType.Sex))?.Id;
            result.IdBodyWeightParameter = sbmlModel.Parameters
                .FirstOrDefault(r => r.IsOfType(PbkModelParameterType.BodyWeight))?.Id;
            result.IdBodySurfaceAreaParameter = sbmlModel.Parameters
                .FirstOrDefault(r => r.IsOfType(PbkModelParameterType.BodySurfaceArea))?.Id;

            return result;
        }
    }
}
