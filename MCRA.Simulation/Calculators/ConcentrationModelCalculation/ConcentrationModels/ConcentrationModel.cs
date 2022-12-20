using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using System;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Base class for a Concentration Model such as CMEmpirical, CMNonDetectSpikeLogNormal, etc.
    /// </summary>
    public abstract class ConcentrationModel {

        /// <summary>
        /// The food of the concentration model.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The substance of the concentration model.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The type of the model.
        /// </summary>
        public abstract ConcentrationModelType ModelType { get; }

        /// <summary>
        /// The concentration unit of the concentrations modelled by this model.
        /// </summary>
        public ConcentrationUnit ConcentrationUnit { get; set; }

        /// <summary>
        /// The model type that was actually desired when generating the model.
        /// Differs from the model type when a fallback is used.
        /// </summary>
        public ConcentrationModelType DesiredModelType { get; set; }

        /// <summary>
        /// Method that is used for handling non-detects.
        /// </summary>
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        /// <summary>
        /// A number between 0 and 1 that can be used when the NonDetectsHandlingMethod is set to 'FractionOfLor'.
        /// <summary>
        public double FractionOfLOR { get; set; }

        /// <summary>
        /// Collection of residues the model is based on.
        /// </summary>
        public CompoundResidueCollection Residues { get; set; }

        /// <summary>
        /// The concentration distribution specified for the food compound of this model.
        /// </summary>
        public ConcentrationDistribution ConcentrationDistribution { get; set; }

        /// <summary>
        /// The MRL recorded for the food/compound combination modelled by this model.
        /// </summary>
        public double MaximumResidueLimit { get; set; }

        /// <summary>
        /// A number between 0 and 1 that can be used by the Mrl model to draw fixed f x MRL.
        /// <summary>
        public double FractionOfMrl { get; set; } = 1;

        /// <summary>
        /// The agricultural use fraction for this food/substance combination.
        /// </summary>
        public double WeightedAgriculturalUseFraction { get; set; } = 1D;

        /// <summary>
        /// The corrected weighted agricultural use fraction based on the fraction of measured positives.
        /// </summary>
        public double CorrectedWeightedAgriculturalUseFraction { get; set; } = 1D;

        /// <summary>
        /// Fraction of the total number of samples that are positives.
        /// </summary>
        public double FractionPositives { get; set; }

        /// <summary>
        /// Fraction of the total number of samples that are true zeros.
        /// </summary>
        public double FractionTrueZeros { get; set; }

        /// <summary>
        /// Fraction of the total number of samples that are censored non-detects, < LOD or  < LOQ.
        /// </summary>
        public double FractionCensored { get; set; }

        /// <summary>
        /// Fraction of the total number of samples that are  non-detects, < LOD.
        /// </summary>
        public double FractionNonDetects { get; set; }

        /// <summary>
        /// Fraction of the total number of samples that are non-quantifications, < LOQ.
        /// </summary>
        public double FractionNonQuantifications { get; set; }

        /// <summary>
        /// Calculates the model parameters based on it's public properties
        /// </summary>
        public abstract bool CalculateParameters();

        /// <summary>
        /// Returns a number based on the model distribution. Overrides the non-detects handling method from the ConcentrationModelSettings.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public abstract double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod);

        /// <summary>
        /// Draws from the positive part of the concentration model; that is, a positive or a positive censored value
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public abstract double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod);

        /// <summary>
        /// Draws a residue according to the non-detects handling method (can be a zero, or a ND replacement value)
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public abstract double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction);

        /// <summary>
        /// Gives the mean of the model distribution using the specified non-detects handling method.
        /// </summary>
        /// <returns></returns>
        public abstract double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod);

        /// <summary>
        /// Gives the mean of the model distribution using the concentration model's non-detects handling method.
        /// </summary>
        /// <returns></returns>
        public double GetDistributionMean() {
            return GetDistributionMean(NonDetectsHandlingMethod);
        }

        /// <summary>
        /// Specifies wheter the model is parametric.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsParametric() {
            return false;
        }

        /// <summary>
        /// Draws a new parameter set for Parametric Uncertainty
        /// </summary>
        public virtual void DrawParametricUncertainty(IRandom random) {
            throw new NotImplementedException();
        }
    }
}
