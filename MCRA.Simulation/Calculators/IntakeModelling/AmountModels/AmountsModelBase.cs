using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.Integrators;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Base class 
    /// </summary>
    public abstract class AmountsModelBase : IAmountsModel {

        public int MinDegreesOfFreedom { get; set; }
        public int MaxDegreesOfFreedom { get; set; }
        public int NumberOfMonteCarloIterations { get; set; }
        public double TestingLevel { get; set; }
        public double VarianceRatio { get; set; }
        public bool IsAcuteCovariateModelling { get; set; }
        public CovariateModelType CovariateModel { get; set; }
        public TestingMethodType TestingMethod { get; set; }
        public TransformType TransformType { get; set; }
        public FunctionType Function { get; set; }

        public abstract AmountsModelSummary CalculateParameters(
            ICollection<SimpleIndividualIntake> individualAmounts,
            List<double> predictionLevels
        );

        public ICollection<ModelledIndividualAmount> GetIndividualAmounts(int seed) {
            return CalculateModelAssistedAmounts(seed);
        }

        public ConditionalPredictionResults GetConditionalPredictions() {
            return GetConditionalPredictionsResults();
        }

        public abstract TransformBase GetDistribution(
            List<ModelledIndividualAmount> predictions,
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        );

        public List<ModelledIndividualAmount> SpecifiedPredictions { get; set; }
        public List<ModelledIndividualAmount> ConditionalPredictions { get; set; }

        protected abstract ICollection<ModelledIndividualAmount> CalculateModelAssistedAmounts(int seed);
        protected abstract ConditionalPredictionResults GetConditionalPredictionsResults();

        /// <summary>
        /// Computes the transformed positive intake amounts.
        /// </summary>
        /// <param name="individualAmounts"></param>
        /// <param name="intakeTransformer"></param>
        /// <returns></returns>
        public static ICollection<ModelledIndividualAmount> ComputeTransformedPositiveIndividualAmounts(
            ICollection<SimpleIndividualIntake> individualAmounts,
            IntakeTransformer intakeTransformer
        ) {
            var result = individualAmounts
                .Where(r => !double.IsNaN(r.Intake) && r.Intake > 0)
                .Select(r => new ModelledIndividualAmount() {
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    Cofactor = r.Cofactor,
                    Covariable = r.Covariable,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    NumberOfPositiveIntakeDays = r.NumberOfPositiveIntakeDays,
                    TransformedAmount = intakeTransformer.Transform(r.Intake),
                    TransformedDayAmounts = r.DayIntakes
                        .Where(da => da > 0)
                        .Select(da => intakeTransformer.Transform(da))
                        .ToArray()
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Checks whether the specified individual amount record matches the covariate group.
        /// </summary>
        /// <param name="individualAmount"></param>
        /// <param name="targetCovariateGroup"></param>
        /// <returns></returns>
        protected bool isMatchCovariateGroup(
            ModelledIndividualAmount individualAmount,
            CovariateGroup targetCovariateGroup
        ) {
            double? covar = double.IsNaN(individualAmount.Covariable) ? null : (double?)individualAmount.Covariable;
            double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : (double?)targetCovariateGroup.Covariable;
            return (individualAmount.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                || (individualAmount.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                || (individualAmount.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                || (individualAmount.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null);
        }
    }
}

