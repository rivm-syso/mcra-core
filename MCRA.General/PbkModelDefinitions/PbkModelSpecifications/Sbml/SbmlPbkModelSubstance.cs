using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelSubstance : IPbkModelSubstanceSpecification {

        /// <summary>
        /// The substance identifier (within the model implementation).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description of the (role of) this substance within this model.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether the substance is an input of the model.
        /// </summary>
        public bool IsInput { get; set; }

    }
}
