using System;
using System.Xml.Linq;
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

            var routes = new List<ExposurePathType>();

            var inputSpecies = sbmlModel.Species
                .Where(r => compartmentsLookup[r.Compartment].IsInputCompartment())
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
                        BiologicalMatrix = r.Key
                    };
                    return result;
                })
                .ToList();

            result.Parameters = sbmlModel.Parameters
                .Select((r, ix) => new KineticModelParameterDefinition() {
                    Id = r.Id,
                    IsInternalParameter = !sbmlModel.AssignmentRules.Any(ar => ar.Variable == r.Id),
                    DefaultValue = r.DefaultValue,
                    Order = ix,
                    Description = r.Name,
                    Unit = r.Units.Equals("UNITLESS", StringComparison.OrdinalIgnoreCase) ? string.Empty : r.Units
                })
                .ToList();

            result.KineticModelSubstances = sbmlModel.Species
                .GroupBy(r => r.GetSubstanceId())
                .Select(g => new KineticModelSubstanceDefinition() {
                    Id = g.Key,
                    IsInput = g.Any(r => compartmentsLookup[r.Compartment].IsInputCompartment()),
                    Name = g.Key
                })
                .ToList();

            result.IdBodyWeightParameter = sbmlModel.Parameters.FirstOrDefault(r => r.IsBodyWeightParameter())?.Id;
            result.IdBodySurfaceAreaParameter = sbmlModel.Parameters.FirstOrDefault(r => r.IsBodySurfaceAreaParameter())?.Id;

            return result;
        }
    }
}
