namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications {
    public interface IPbkModelSpecification {

        /// <summary>
        /// Model identier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name of the model.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the model.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Model format.
        /// </summary>
        PbkModelType Format { get; }

        /// <summary>
        /// Model implementation filename.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// The model time resolution.
        /// </summary>
        TimeUnit Resolution { get; }

        /// <summary>
        /// The evaluation frequency per resolution.
        /// </summary>
        int EvaluationFrequency { get; }

        /// <summary>
        /// Retrieves the list of substances defined within the PBK model.
        /// </summary>
        List<IPbkModelSubstanceSpecification> GetModelSubstances();

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        List<IPbkModelOutputSpecification> GetOutputs();

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        List<BiologicalMatrix> GetOutputMatrices();

        /// <summary>
        /// Retrieves the list of parameters defined within the PBK model.
        /// </summary>
        List<IPbkModelParameterSpecification> GetParameters();

        /// <summary>
        /// Gets a parameter definition by its type.
        /// </summary>
        IPbkModelParameterSpecification GetParameterDefinitionByType(PbkModelParameterType paramType);

        /// <summary>
        /// Returns true if this is a lifetime model, i.e. it has an (external) initial age
        /// parameter and an (internal/variable) age parameter.
        /// </summary>
        bool IsLifetimeModel();

    }
}
