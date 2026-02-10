namespace MCRA.General {
    [Serializable]
    public class PbkModelSubstanceParameterSpecification {

        /// <summary>
        /// The substance identifier (within the model).
        /// </summary>
        public string IdSubstance { get; set; }

        /// <summary>
        /// Gets/sets the name of the substance.
        /// </summary>
        public string IdParameter { get; set; }

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public double? DefaultValue { get; set; }

        /// <summary>
        /// Index in C code (dll)
        /// </summary>
        public int Order { get; set; }

    }
}
