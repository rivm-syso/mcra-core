using System.Xml.Serialization;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve {

    [Serializable]
    public class DeSolvePbkModelOutputSpecification : IPbkModelOutputSpecification {

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
        /// Gets/sets the compartment type that is associated with
        /// this output.
        /// </summary>
        public PbkModelCompartmentType CompartmentType { get; set; } = PbkModelCompartmentType.Undefined;

        /// <summary>
        /// Gets/sets the description of this parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The output type: concentration/amount/cumulative amount.
        /// </summary>
        public PbkModelOutputType Type { get; set; }

        /// <summary>
        /// Gets/sets the unit of this parameter.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Index in C code (dll).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Compartment size parameter.
        /// </summary>
        public string CompartmentSizeParameter { get; set; }

        /// <summary>
        /// Scaling factors
        /// </summary>
        [XmlArrayItem("ScalingFactor")]
        public List<string> ScalingFactors { get; set; }

        /// <summary>
        /// Scaling factors
        /// </summary>
        [XmlArrayItem("MultiplicationFactor")]
        public List<double> MultiplicationFactors { get; set; }

        /// <summary>
        /// Returns the unit of this output.
        /// </summary>
        public TargetUnit TargetUnit {
            get {
                var target = BiologicalMatrix != BiologicalMatrix.Undefined
                    ? new ExposureTarget(BiologicalMatrix)
                    : null;
                var doseUnit = DoseUnitConverter.FromString(Unit);
                var unit = ExposureUnitTriple.FromDoseUnit(doseUnit);
                var result = new TargetUnit() {
                    Target = target,
                    ExposureUnit = unit
                };
                return result;
            }
        }
    }
}
