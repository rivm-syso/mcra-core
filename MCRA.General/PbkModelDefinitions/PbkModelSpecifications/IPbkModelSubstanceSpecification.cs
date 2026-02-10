namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications {

    public interface IPbkModelSubstanceSpecification {

        /// <summary>
        /// The substance identifier (within the model implementation).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Description of the (role of) this substance within this model.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Specifies whether the substance is an input of the model.
        /// </summary>
        public bool IsInput { get; }

    }
}
