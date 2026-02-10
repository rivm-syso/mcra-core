namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {

    public class SbmlPbkModelOutputSpecification : IPbkModelOutputSpecification {

        /// <summary>
        /// Gets/sets the output id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the model substance associated with the output.
        /// </summary>
        public string IdSubstance { get; set; }

        /// <summary>
        /// Identifier of the model compartment associated with the output.
        /// </summary>
        public string IdCompartment { get; set; }

        /// <summary>
        /// Gets/sets the biological matrix that is associated with
        /// this output (if available).
        /// </summary>
        public BiologicalMatrix BiologicalMatrix { get; set; }

        /// <summary>
        /// The output type: concentration/amount/cumulative amount.
        /// </summary>
        public PbkModelOutputType Type { get; set; }

        /// <summary>
        /// Returns the unit of this output.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

    }
}
