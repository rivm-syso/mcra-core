namespace MCRA.General {
    [Serializable]
    public class PbkModelInputSpecification {

        /// <summary>
        /// Gets/sets the parameter id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the description of this parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The exposure.
        /// </summary>
        public ExposureRoute Route { get; set; }

        /// <summary>
        /// Gets/sets the unit of this parameter.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Index in C code (dll).
        /// </summary>
        public int Order { get; set; }

        public DoseUnit DoseUnit {
            get {
                return DoseUnitConverter.FromString(Unit);
            }
        }
    }
}
