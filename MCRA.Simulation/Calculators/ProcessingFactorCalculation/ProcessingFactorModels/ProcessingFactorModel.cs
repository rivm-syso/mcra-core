using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {

    /// <summary>
    /// Base class for fixed and distribution based (lognormal, logistic normalprocessing factors)
    /// </summary>
    public abstract class ProcessingFactorModel {

        protected bool _isModellingUncertainty;

        public ProcessingFactor ProcessingFactor { get; private set; }

        public ProcessingFactorModel(ProcessingFactor processingFactor) {
            ProcessingFactor = processingFactor;
        }

        /// <summary>
        /// The (raw) food for which this processing factor model is defined.
        /// </summary>
        public Food Food => ProcessingFactor.FoodUnprocessed;

        /// <summary>
        /// The substance for which this processing factor model is defined.
        /// </summary>
        public Compound Substance => ProcessingFactor.Compound;

        /// <summary>
        /// The processing type for which this processing factor model is defined.
        /// </summary>
        public ProcessingType ProcessingType => ProcessingFactor.ProcessingType;

        /// <summary>
        /// Calculate the model parameters.
        /// </summary>
        public abstract void CalculateParameters();

        /// <summary>
        /// Returns the nominal (variability) value from the processing factor model.
        /// </summary>
        /// <returns></returns>
        public abstract double GetNominalValue();

        /// <summary>
        /// Draws a processing factor from the processing factor distribution.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public abstract double DrawFromDistribution(IRandom random);

        /// <summary>
        /// Resamples the model parameters for use in uncertainty runs.
        /// </summary>
        /// <param name="random"></param>
        public abstract void Resample(IRandom random);

        /// <summary>
        /// Resets the processing factor model such to draw factors for the
        /// nominal run.
        /// </summary>
        public abstract void ResetNominal();

        /// <summary>
        /// Returns true if this processing factor model is a sample within an
        /// uncertainty loop.
        /// </summary>
        /// <returns></returns>
        public bool IsUncertaintySample() {
            return _isModellingUncertainty;
        }

        public abstract bool GetApplyProcessingCorrectionFactor();
    }
}
