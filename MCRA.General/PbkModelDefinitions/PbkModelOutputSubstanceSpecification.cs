namespace MCRA.General {

    [Serializable]
    public class PbkModelOutputSubstanceSpecification {

        /// <summary>
        /// Gets/sets the output (species) id.
        /// </summary>
        public string IdSpecies { get; set; }

        /// <summary>
        /// Gets/sets the id of the substance to which the output (species)
        /// is linked.
        /// </summary>
        public string IdSubstance { get; set; }

    }
}
