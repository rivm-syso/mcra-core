namespace MCRA.General {
    [Serializable]
    public class PbkModelSubstanceSpecification {

        /// <summary>
        /// The substance identifier (within the model).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the name of the substance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the description of the role of this substance
        /// within this model.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether the substance is an input of the model.
        /// </summary>
        public bool IsInput { get; set; } = true;

    }
}
