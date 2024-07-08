using System.Xml.Serialization;

namespace MCRA.General {

    [Serializable]
    public class KineticModelParameterDefinition {

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

        /// <summary>
        /// Parameter type
        /// </summary>
        public PbkModelParameterType Type { get; set; }

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public double? DefaultValue { get; set; }

        /// <summary>
        /// If true, then this parameter is considered a local parameter
        /// that is only used internally (by the model implementation).
        /// </summary>
        [XmlAttribute]
        public bool IsInternalParameter { get; set; }

        /// <summary>
        /// Substance parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("SubstanceParameterValue")]
        public List<KineticModelSubstanceParameterDefinition> SubstanceParameterValues { get; set; }
    }
}
