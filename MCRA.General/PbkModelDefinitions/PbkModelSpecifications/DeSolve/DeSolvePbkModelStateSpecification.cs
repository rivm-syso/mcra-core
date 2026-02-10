namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve {

    [Serializable]
    public class DeSolvePbkModelStateSpecification {

        /// <summary>
        /// Identifier of the state variable.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the model substance associated with the state variable.
        /// </summary>
        public string IdSubstance { get; set; }

        /// <summary>
        /// Description of this state variable.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unit of this state variable.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Order number of the state variable in C code.
        /// </summary>
        public int? Order { get; set; }

    }
}
