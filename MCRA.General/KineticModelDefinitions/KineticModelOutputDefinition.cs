using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCRA.General {

    public enum KineticModelOutputType {
        Concentration,
        CumulativeAmount
    }

    [Serializable]
    public class KineticModelOutputDefinition {

        /// <summary>
        /// Gets/sets the parameter id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the biological matrix that is associated with
        /// this output (if available).
        /// </summary>
        public string BiologicalMatrix { get; set; }

        /// <summary>
        /// Gets/sets the description of this parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The output type: concentration/amount/cumulative amount.
        /// </summary>
        public KineticModelOutputType Type { get; set; }

        /// <summary>
        /// Gets/sets the unit of this parameter.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Index in C code (dll).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Scaling factors
        /// </summary>
        [XmlArrayItem("ScalingFactor")]
        public List<string> ScalingFactors { get; set; }

        /// <summary>
        /// Scaling factors
        /// </summary>
        [XmlArrayItem("MultiplicationFactors")]
        public List<double> MultiplicationFactors { get; set; }

        /// <summary>
        /// Substances
        /// </summary>
        [XmlArrayItem("Substance")]
        public List<string> Substances { get; set; }

        /// <summary>
        /// Returns the dose unit of this output.
        /// </summary>
        public DoseUnit DoseUnit {
            get {
                return DoseUnitConverter.FromString(Unit);
            }
        }
    }
}
