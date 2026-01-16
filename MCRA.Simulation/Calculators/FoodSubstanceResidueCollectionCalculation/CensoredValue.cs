using MCRA.General;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {
    public sealed class CensoredValue {

        /// <summary>
        /// The LOD.
        /// </summary>
        public double LOD { get; set; }

        /// <summary>
        /// The LOQ.
        /// </summary>
        public double LOQ { get; set; }

        /// <summary>
        /// The type of the censored value.
        /// </summary>
        public ResType ResType { get; set; }
    }
}
