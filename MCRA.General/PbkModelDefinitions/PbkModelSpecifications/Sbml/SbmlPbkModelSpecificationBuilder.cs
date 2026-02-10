using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml.Extensions;
using MCRA.Utils.Sbml.Objects;
using MCRA.Utils.SBML;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelSpecificationBuilder {

        /// <summary>
        /// Creates a <see cref="SbmlPbkModelSpecification" /> instance from an SBML file.
        /// </summary>
        public static SbmlPbkModelSpecification CreateFromSbmlFile(string filePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"Specified path {filePath} not found");
            }
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filePath);
            var modelDefinition = Create(sbmlModel);
            modelDefinition.Id = sbmlModel.Id;
            modelDefinition.FileName = filePath;
            return modelDefinition;
        }

        public static SbmlPbkModelSpecification Create(SbmlModel sbmlModel) {
            if (sbmlModel.Compartments.Count == 0) {
                throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: model does not contain any compartment.");
            }
            if (sbmlModel.Species.Count == 0) {
                throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: model does not contain any chemical species.");
            }
            var timeUnit = getModelTimeUnit(sbmlModel);
            var compartments = createModelCompartments(sbmlModel);
            var species = createModelSpecies(sbmlModel, compartments);
            var modelSubstances = createModelSubstances(species);
            var parameters = createModelParameters(sbmlModel);
            var result = new SbmlPbkModelSpecification {
                Id = sbmlModel.Id,
                Name = sbmlModel.Name ?? sbmlModel.Id,
                SbmlModel = sbmlModel,
                EvaluationFrequency = 1,
                Resolution = timeUnit,
                ModelSubstances = modelSubstances,
                Compartments = compartments,
                Species = species,
                Parameters = parameters,
            };
            return result;
        }

        public static TimeUnit getModelTimeUnit(SbmlModel model) {
            if (!string.IsNullOrEmpty(model.TimeUnits)) {
                if (!model.UnitDefinitions.TryGetValue(model.TimeUnits, out var unit)) {
                    throw new Exception($"Error reading SBML model [{model.Id}]: unit definition not found for model time unit [{model.TimeUnits}].");
                }
                return unit.ToTimeUnit();
            }
            throw new Exception($"Error reading SBML model [{model.Id}]: time unit not specified.");
        }

        private static List<SbmlPbkModelCompartment> createModelCompartments(
            SbmlModel sbmlModel
        ) {
            return sbmlModel.Compartments.Values
                .Select(c => {
                    if (string.IsNullOrEmpty(c.Units)) {
                        throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: no unit specified for compartment [{c.Id}].");
                    }
                    if (!sbmlModel.UnitDefinitions.TryGetValue(c.Units, out var unitDef)) {
                        throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: unit definition [{c.Units}] specified for compartment [{c.Id}] not found in model unit definitions.");
                    }
                    return new SbmlPbkModelCompartment() {
                        SbmlModelCompartment = c,
                        Unit = unitDef.ToConcentrationMassUnit(),
                    };
                })
                .ToList();
        }

        private static List<SbmlPbkModelSpecies> createModelSpecies(
            SbmlModel sbmlModel,
            List<SbmlPbkModelCompartment> compartments
        ) {
            var compartmentsLookup = compartments.ToDictionary(r => r.Id);
            return sbmlModel.Species.Values
                .Select(r => {
                    if (string.IsNullOrEmpty(r.Compartment)) {
                        throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: no compartment specified for species [{r.Id}].");
                    }
                    if (!compartmentsLookup.TryGetValue(r.Compartment, out var compartment)) {
                        throw new SbmlPbkModelException($"Error reading SBML model [{sbmlModel.Id}]: compartment [{r.Compartment}] specified for species [{r.Id}] not found in model compartments.");
                    }
                    var amountUnit = sbmlModel.UnitDefinitions[r.SubstanceUnits].ToSubstanceAmountUnit();
                    var result = new SbmlPbkModelSpecies() {
                        SbmlModelSpecies = r,
                        Compartment = compartment,
                        SubstanceAmountUnit = amountUnit
                    };
                    return result;
                })
                .ToList();
        }

        private static List<SbmlPbkModelParameter> createModelParameters(SbmlModel sbmlModel) {
            return sbmlModel.Parameters.Values
                .Select((r, ix) => new SbmlPbkModelParameter() {
                    SbmlModelParameter = r,
                    SbmlUnit = !string.IsNullOrEmpty(r.Units)
                        && sbmlModel.UnitDefinitions.TryGetValue(r.Units, out var unitDef)
                        ? unitDef : null
                })
                .ToList();
        }

        private static List<SbmlPbkModelSubstance> createModelSubstances(List<SbmlPbkModelSpecies> species) {
            return species
                .GroupBy(r => r.GetSubstanceId())
                .Select(g => new SbmlPbkModelSubstance() {
                    Id = g.Key,
                    Description = g.Key,
                    IsInput = !g.First().IsMetaboliteSpecies(),
                })
                .ToList();
        }
    }
}
