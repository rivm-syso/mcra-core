namespace MCRA.General {
    public interface IPbkModelSpecification {

        /// <summary>
        /// Gets/sets the unit id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets/sets the unit name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets/sets the description of the name.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets/sets the file name and extension of the underlying kinetic model engine.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Identifier of the main model (a definition is a specific version
        /// of this main model).
        /// </summary>
        string IdModel { get; }

        /// <summary>
        /// The version of this definition.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets/sets the PBK model type.
        /// </summary>
        PbkModelType Format { get; }

        /// <summary>
        /// The kinetic model resolution.
        /// </summary>
        TimeUnit Resolution { get; }

        /// <summary>
        /// The evaluation frequency per resolution.
        /// </summary>
        int EvaluationFrequency { get; }

        /// <summary>
        /// The integrator to use, either a function that performs integration, or a list
        /// of class rkMethod. The default is 'lsoda'.
        /// </summary>
        string IdIntegrator { get; }

        /// <summary>
        /// Retrieves the list of inputs available in the PBK model.
        /// </summary>
        List<PbkModelInputSpecification> GetInputDefinitions();

        /// <summary>
        /// Returns the exposure routes of the PBK model definition.
        /// </summary>
        ICollection<ExposureRoute> GetExposureRoutes();

        /// <summary>
        /// Retrieves the list of substances defined within the PBK model.
        /// </summary>
        List<PbkModelSubstanceSpecification> GetModelSubstances();

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        List<PbkModelOutputSpecification> GetOutputs();

        /// <summary>
        /// Retrieves the list of states defined within the PBK model.
        /// </summary>
        List<PbkModelStateVariableSpecification> GetStates();

        /// <summary>
        /// Retrieves the list of parameters defined within the PBK model.
        /// </summary>
        List<PbkModelParameterSpecification> GetParameters();

        /// <summary>
        /// Gets a parameter definition by its type.
        /// </summary>
        PbkModelParameterSpecification GetParameterDefinitionByType(PbkModelParameterType paramType);

        /// <summary>
        /// Returns true if this is a lifetime model, i.e. it has an (external) initial age
        /// parameter and an (internal/variable) age parameter.
        /// </summary>
        bool IsLifetimeModel();

    }
}
