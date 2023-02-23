using System.Xml.Serialization;

namespace MCRA.General {
    [Serializable]
    public class KineticModelStateVariableDefinition {
        /// <summary>
        /// Gets/sets the parameter id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the description of this parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the unit of this parameter.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Index in C code (dll)
        /// </summary>
        public int? Order { get; set; }

        public string IdSubstance { get; set; }

        /// <summary>
        /// Substance parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("StateSubstance")]
        public List<KineticModelStateSubstanceDefinition> StateSubstances { get; set; }
    }
}
