namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications {
    public interface IPbkModelOutputSpecification {

        /// <summary>
        /// Output identifier.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Identifier of the model substance associated with the output.
        /// </summary>
        public string IdSubstance { get; set; }

        /// <summary>
        /// Identifier of the model compartment associated with the output.
        /// </summary>
        public string IdCompartment { get; set; }

        /// <summary>
        /// The biological matrix associated with this output (if available).
        /// </summary>
        BiologicalMatrix BiologicalMatrix { get; set; }

        /// <summary>
        /// The output type: concentration/amount/cumulative amount.
        /// </summary>
        PbkModelOutputType Type { get; set; }

        /// <summary>
        /// Returns the unit of this output.
        /// </summary>
        TargetUnit TargetUnit { get; }
    }
}
