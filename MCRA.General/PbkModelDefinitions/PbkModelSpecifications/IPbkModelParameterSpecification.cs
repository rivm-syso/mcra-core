namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications {

    public interface IPbkModelParameterSpecification {

        /// <summary>
        /// The parameter id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets/sets the description of this parameter.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets/sets the unit of this parameter.
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// Index in C code (dll)
        /// </summary>
        public int? Order { get; }

        /// <summary>
        /// Parameter type
        /// </summary>
        public PbkModelParameterType Type { get; }

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public double? DefaultValue { get; }

        /// <summary>
        /// If true, then this parameter is considered a local parameter
        /// that is only used internally (by the model implementation).
        /// </summary>
        public bool IsInternalParameter { get; }

    }
}
