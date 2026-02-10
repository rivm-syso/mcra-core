using System.Xml.Serialization;

namespace MCRA.General {

    [Serializable]
    [XmlType("PbkModelSpecification")]
    public class EmbeddedPbkModelSpecification : UnitValueDefinition, IPbkModelSpecification {

        /// <summary>
        /// Identifier of the main model (a definition is a specific version of this main model).
        /// </summary>
        public string IdModel { get; set; }

        /// <summary>
        /// The version of this definition.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets/sets the PBK model type.
        /// </summary>
        public PbkModelType Format { get; set; }

        /// <summary>
        /// Gets/sets the file name and extension of the underlying PBK model implementation.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The kinetic model resolution.
        /// </summary>
        public TimeUnit Resolution { get; set; }

        /// <summary>
        /// The evaluation frequency per resolution.
        /// </summary>
        public int EvaluationFrequency { get; set; }

        /// <summary>
        /// The integrator to use, either a function that performs integration, or a list
        /// of class rkMethod. The default is 'lsoda'.
        /// </summary>
        public string IdIntegrator { get; set; }

        /// <summary>
        /// Forcing parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("ModelSubstance")]
        public List<PbkModelSubstanceSpecification> ModelSubstances { get; set; }

        /// <summary>
        /// Forcing parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Forcing")]
        public List<PbkModelInputSpecification> Forcings { get; set; }

        /// <summary>
        /// Input parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Parameter")]
        public List<PbkModelParameterSpecification> Parameters { get; set; }

        /// <summary>
        /// State parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("State")]
        public List<PbkModelStateVariableSpecification> States { get; set; }

        /// <summary>
        /// Output parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Output")]
        public List<PbkModelOutputSpecification> Outputs { get; set; }

        /// <summary>
        /// Retrieves the list of inputs available in the PBK model.
        /// </summary>
        public List<PbkModelInputSpecification> GetInputDefinitions() {
            return Forcings;
        }

        /// <summary>
        /// Retrieves the list of substances defined within the PBK model.
        /// </summary>
        public List<PbkModelSubstanceSpecification> GetModelSubstances() {
            return ModelSubstances;
        }

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        public List<PbkModelOutputSpecification> GetOutputs() {
            return Outputs;
        }

        /// <summary>
        /// Retrieves the list of states defined within the PBK model.
        /// </summary>
        public List<PbkModelStateVariableSpecification> GetStates() {
            return States;
        }

        /// <summary>
        /// Retrieves the list of parameters defined within the PBK model.
        /// </summary>
        public List<PbkModelParameterSpecification> GetParameters() {
            return Parameters;
        }

        /// <summary>
        /// Returns the exposure routes of the PBK model definition (ordered by
        /// the order of the forcings).
        /// </summary>
        public ICollection<ExposureRoute> GetExposureRoutes() {
            return Forcings.OrderBy(c => c.Order).Select(c => c.Route).ToList();
        }

        /// <summary>
        /// Returns true if this is a lifetime model, i.e. it has an (external) initial age
        /// parameter and an (internal/variable) age parameter.
        /// </summary>
        public bool IsLifetimeModel() {
            return Parameters.Any(p => p.Type == PbkModelParameterType.Age && p.IsInternalParameter)
                && Parameters.Any(p => p.Type == PbkModelParameterType.AgeInit && !p.IsInternalParameter);
        }

        /// <summary>
        /// Gets a parameter definition by its type.
        /// </summary>
        public PbkModelParameterSpecification GetParameterDefinitionByType(PbkModelParameterType paramType) {
            return Parameters.FirstOrDefault(p => p.Type == paramType);
        }
    }
}
