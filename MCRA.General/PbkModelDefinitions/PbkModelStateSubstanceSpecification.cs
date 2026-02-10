namespace MCRA.General {
    [Serializable]
    public class PbkModelStateSubstanceSpecification {

        /// <summary>
        /// The state identifier (within the model).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the name of the substance.
        /// </summary>
        public string IdSubstance { get; set; }

        /// <summary>
        /// Index in C code (dll)
        /// </summary>
        public int Order { get; set; }

    }
}
