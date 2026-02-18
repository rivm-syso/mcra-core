using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelSpecification : IPbkModelSpecification {

        /// <summary>
        /// Identifier of the model.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the model.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the PBK model type.
        /// </summary>
        public PbkModelType Format => PbkModelType.SBML;

        /// <summary>
        /// Gets/sets the file name and extension of the underlying PBK model implementation.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// SBML model definition.
        /// </summary>
        public SbmlModel SbmlModel { get; set; }

        /// <summary>
        /// The kinetic model resolution.
        /// </summary>
        public TimeUnit Resolution { get; set; }

        /// <summary>
        /// The evaluation frequency per resolution.
        /// </summary>
        public int EvaluationFrequency { get; set; }

        /// <summary>
        /// Substances defined within the PBK model.
        /// </summary>
        public List<SbmlPbkModelSubstance> ModelSubstances { get; set; }

        /// <summary>
        /// SBML PBK model compartments.
        /// </summary>
        public List<SbmlPbkModelCompartment> Compartments { get; set; }

        /// <summary>
        /// SBML PBK model chemical species.
        /// </summary>
        public List<SbmlPbkModelSpecies> Species { get; set; }

        /// <summary>
        /// Input parameters of the PBK model.
        /// </summary>
        public List<SbmlPbkModelParameter> Parameters { get; set; }

        /// <summary>
        /// Retrieves the list of inputs available in the PBK model.
        /// </summary>
        public Dictionary<ExposureRoute, SbmlPbkModelSpecies> GetRouteInputSpecies(bool fallbackSystemic = false) {
            var result = new Dictionary<ExposureRoute, SbmlPbkModelSpecies>();
            var routes = new[] { ExposureRoute.Inhalation, ExposureRoute.Oral, ExposureRoute.Dermal };
            foreach (var route in routes) {
                var inputForRoute = Species
                    .Select(r => (species: r, priority: r.Compartment.GetExposureRouteInputPriority(route)))
                    .Where(c => c.priority > 0)
                    .OrderByDescending(c => c.priority)
                    .Select(r => r.species)
                    .FirstOrDefault();
                if (inputForRoute == null && fallbackSystemic) {
                    inputForRoute = Species
                    .Select(r => (species: r, priority: r.Compartment.GetSystemicInputPriority()))
                    .Where(c => c.priority > 0)
                    .OrderByDescending(c => c.priority)
                    .Select(r => r.species)
                    .FirstOrDefault();
                }
                if (inputForRoute != null) {
                    result.Add(route, inputForRoute);
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the list of substances defined within the PBK model.
        /// </summary>
        public List<IPbkModelSubstanceSpecification> GetModelSubstances() {
            var result = ModelSubstances
                .Cast<IPbkModelSubstanceSpecification>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        public List<IPbkModelOutputSpecification> GetOutputs() {
            var result = Species
                .Select(r => {
                    var amountUnit = r.SubstanceAmountUnit;
                    var massUnit = r.Compartment.Unit;
                    var biologicalMatrix = r.Compartment.GetBiologicalMatrix();
                    var target = biologicalMatrix != BiologicalMatrix.Undefined
                        ? new ExposureTarget(biologicalMatrix)
                        : null;
                    var unit = new ExposureUnitTriple(amountUnit, massUnit);
                    var result = new SbmlPbkModelOutputSpecification() {
                        Id = r.Id,
                        IdCompartment = r.Compartment.Id,
                        IdSubstance = r.IdSubstance,
                        Type = PbkModelOutputType.Concentration,
                        TargetUnit = new TargetUnit(target, unit),
                        BiologicalMatrix = r.Compartment.GetBiologicalMatrix()
                    };
                    return result;
                })
                .Cast<IPbkModelOutputSpecification>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Returns the biological matrices for which this PBK model provides outputs.
        /// </summary>
        /// <returns></returns>
        public List<BiologicalMatrix> GetOutputMatrices() {
            return Species
                .Select(r => r.Compartment.BiologicalMatrix)
                .Where(r => r != BiologicalMatrix.Undefined)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Retrieves the list of parameters defined within the PBK model.
        /// </summary>
        public List<IPbkModelParameterSpecification> GetParameters() {
            var result = Parameters.Cast<IPbkModelParameterSpecification>().ToList();
            return result;
        }

        /// <summary>
        /// Returns true if this is a lifetime model, i.e. it has an (external) initial age
        /// parameter and an (internal/variable) age parameter.
        /// </summary>
        public bool IsLifetimeModel() {
            return Parameters.Any(p => p.Type == PbkModelParameterType.Age && p.IsVariable)
                && Parameters.Any(p => p.Type == PbkModelParameterType.AgeInit && p.IsConstant);
        }

        /// <summary>
        /// Gets a parameter definition by its type.
        /// </summary>
        public IPbkModelParameterSpecification GetParameterDefinitionByType(PbkModelParameterType paramType) {
            return Parameters.FirstOrDefault(p => p.Type == paramType);
        }
    }
}
