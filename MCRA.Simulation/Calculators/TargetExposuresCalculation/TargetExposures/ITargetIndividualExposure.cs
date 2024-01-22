using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualExposure : ITargetExposure {

        /// <summary>
        /// The id of the individual used during simulation runs.
        /// </summary>
        int SimulatedIndividualId { get; }
        
        /// <summary>
        /// The sampling weight of the individual.
        /// </summary>
        double IndividualSamplingWeight { get; }

        /// <summary>
        /// The body weight of the individual as used in calculations, which is most of the times equal to the 
        /// original individual body weight read from the data or an imputed value when the body weight is missing.
        /// </summary>
        double SimulatedIndividualBodyWeight { get; }

        /// <summary>
        /// Get the exposure value, a concentration or an absolute amount, for the specified substance.
        /// </summary>
        double GetExposureForSubstance(Compound compound);

        /// <summary>
        /// Returns all the substances for this target exposure.
        /// </summary>
        ICollection<Compound> Substances { get; }
    }
}
