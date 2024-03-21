using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {
    public class SbmlToPbkModelDefinitionConverter {

        public KineticModelDefinition Convert(SbmlModel sbmlModel) {
            var result = new KineticModelDefinition();
            var unitsDictionary = sbmlModel.UnitDefinitions;
            var compartmentsLookup = sbmlModel.Compartments
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);

            result.Format = PbkImplementationFormat.SBML;
            result.EvaluationFrequency = 1;
            result.Resolution = TimeUnit.Hours.ToString();

            result.Forcings = sbmlModel.Species
                .Where(r => compartmentsLookup[r.Compartment].IsInputCompartment())
                .Select(r => {
                    var amountUnit = unitsDictionary[r.SubstanceUnits].ToSubstanceAmountUnit();
                    var concentrationMassUnit = unitsDictionary[compartmentsLookup[r.Compartment].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var result = new KineticModelInputDefinition() {
                        Id = r.Id,
                        Description = r.Name,
                        Route = compartmentsLookup[r.Compartment].GetExposurePathType(),
                        Unit = doseUnit,
                    };
                    return result;
                })
                .ToList();

            result.Outputs = sbmlModel.Species
                .GroupBy(r => compartmentsLookup[r.Compartment].Id)
                .Select(r => {
                    // TODO: assuming same substance unit for all species
                    var amountUnit = unitsDictionary[r.First().SubstanceUnits].ToSubstanceAmountUnit();
                    var concentrationMassUnit = unitsDictionary[compartmentsLookup[r.Key].Units].ToConcentrationMassUnit();
                    var doseUnit = $"{amountUnit.GetShortDisplayName()}/{concentrationMassUnit.GetShortDisplayName()}";
                    var result = new KineticModelOutputDefinition() {
                        Id = compartmentsLookup[r.Key].Id,
                        Type = KineticModelOutputType.Concentration,
                        Unit = doseUnit,
                        Substances = r.Select(r => r.Id).ToList()
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
                })
                .ToList();

            result.IdBodyWeightParameter = sbmlModel.Parameters.FirstOrDefault(r => r.IsBodyWeightParameter())?.Id;
            result.IdBodySurfaceAreaParameter = sbmlModel.Parameters.FirstOrDefault(r => r.IsBodySurfaceAreaParameter())?.Id;

            return result;
        }
    }
}
