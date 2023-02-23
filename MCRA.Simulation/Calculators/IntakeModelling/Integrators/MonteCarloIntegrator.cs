using MCRA.Utils;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class MonteCarloIntegrator<TFrequencyModel, TAmountsModel>
        where TFrequencyModel : IFrequencyModel
        where TAmountsModel : IAmountsModel {

        public TFrequencyModel FrequencyModel { get; set; }
        public TAmountsModel AmountsModel { get; set; }

        public int NumberOfMonteCarloIterations { get; set; }

        private int monteCarloNumberOfIterations = 100000;

        /// <summary>
        /// Calculates conditional for specified predictions usual exposures
        /// </summary>
        /// <returns></returns>
        public List<ConditionalUsualIntake> CalculateConditionalIntakes(
            List<CovariateGroup> predictionCovariateGroups,
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (NumberOfMonteCarloIterations > 0 && NumberOfMonteCarloIterations < 100000) {
                monteCarloNumberOfIterations = NumberOfMonteCarloIterations;
            }
            List<IndividualFrequency> frequencyPredictions;
            if (FrequencyModel is BetaBinomialFrequencyModel) {
                frequencyPredictions = (FrequencyModel as BetaBinomialFrequencyModel).SpecifiedPredictions;
            } else {
                frequencyPredictions = (FrequencyModel as LogisticNormalFrequencyModel).SpecifiedPredictions;
            }
            var amountsPredictions = (AmountsModel as NormalAmountsModel).SpecifiedPredictions;
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var results = predictionCovariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = Simulation.IsBackwardCompatibilityMode
                        ? new McraRandomGenerator(covariateGroup.GetHashCode() + seed, true)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    (var frequencyDistribution, var fcg) = FrequencyModel.GetDistribution(frequencyPredictions, covariateGroup);
                    var amountsDistribution = AmountsModel.GetDistribution(amountsPredictions, covariateGroup, out var acg);
                    var usualIntakes = new List<double>();
                    for (int i = 0; i < monteCarloNumberOfIterations; i++) {
                        usualIntakes.Add(frequencyDistribution.Draw(random) * amountsDistribution.Draw(random));
                    }
                    return new ConditionalUsualIntake() {
                        CovariatesCollection = new CovariatesCollection() {
                            FrequencyCofactor = fcg.Cofactor ?? string.Empty,
                            FrequencyCovariable = fcg.Covariable,
                            AmountCofactor = acg.Cofactor ?? string.Empty,
                            AmountCovariable = acg.Covariable,
                        },
                        CovariateGroup = covariateGroup,
                        ConditionalUsualIntakes = usualIntakes,
                    };
                })
            .ToList();

            if (results.Select(c => c.CovariatesCollection.AmountCofactor).First() != string.Empty) {
                return results.OrderBy(c => c.CovariatesCollection.AmountCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)
                        .ToList();
            } else if (results.Select(c => c.CovariatesCollection.FrequencyCofactor).First() != string.Empty) {
                return results.OrderBy(c => c.CovariatesCollection.FrequencyCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                        .ToList();
            } else {
                return results.OrderBy(c => c.CovariatesCollection.FrequencyCovariable)
                         .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                         .ToList();
            }
        }

        /// <summary>
        /// Calculates marginal usual exposures
        /// </summary>
        /// <returns></returns>
        public List<ModelBasedIntakeResult> CalculateMarginalIntakes(
            List<CovariateGroup> covariateGroups,
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (NumberOfMonteCarloIterations > 0 && NumberOfMonteCarloIterations < 100000) {
                monteCarloNumberOfIterations = NumberOfMonteCarloIterations;
            }
            List<IndividualFrequency> frequencyPredictions;
            if (FrequencyModel is BetaBinomialFrequencyModel) {
                frequencyPredictions = (FrequencyModel as BetaBinomialFrequencyModel).ConditionalPredictions;
            } else {
                frequencyPredictions = (FrequencyModel as LogisticNormalFrequencyModel).ConditionalPredictions;
            }
            var amountsPredictions = (AmountsModel as NormalAmountsModel).ConditionalPredictions;

            var n = monteCarloNumberOfIterations / covariateGroups.Sum(c => c.GroupSamplingWeight);
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            if (!covariateGroups.Any()) {
                return new List<ModelBasedIntakeResult>() { new ModelBasedIntakeResult(){
                    CovariateGroup = null,
                    ModelBasedIntakes = Enumerable.Repeat(0D, monteCarloNumberOfIterations).ToList(),
                    }
                };
            } else {
                return covariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = Simulation.IsBackwardCompatibilityMode
                        ? new McraRandomGenerator(covariateGroup.GetHashCode() + seed, true)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    (var frequencyDistribution, var fcg) = FrequencyModel.GetDistribution(frequencyPredictions, covariateGroup);
                    var amountsDistribution = AmountsModel.GetDistribution(amountsPredictions, covariateGroup, out var acg);
                    var usualIntakes = new List<double>();
                    var nSimulations = BMath.Ceiling(covariateGroup.GroupSamplingWeight * n);
                    for (int i = 0; i < nSimulations; i++) {
                        usualIntakes.Add(frequencyDistribution.Draw(random) * amountsDistribution.Draw(random));
                    }
                    return new ModelBasedIntakeResult() {
                        CovariateGroup = covariateGroup,
                        ModelBasedIntakes = usualIntakes,
                    };
                }).ToList();
            }
        }

        /// <summary>
        /// Calculates individual usual exposures, only for BBN/LNN
        /// </summary>
        /// <returns></returns>
        public List<ModelAssistedIntake> CalculateIndividualIntakes(int seed) {
            var gif = FrequencyModel.GetIndividualFrequencies();
            var gia = AmountsModel.GetIndividualAmounts(seed);
            var usualIntakeIndividuals = gif.Join(gia,
               f => f.SimulatedIndividualId,
               a => a.SimulatedIndividualId,
                   (f, a) => {
                       return new ModelAssistedIntake {
                           IndividualSamplingWeight = a.IndividualSamplingWeight,
                           UsualIntake = f.ModelAssistedFrequency * a.BackTransformedAmount,
                           SimulatedIndividualId = f.SimulatedIndividualId,
                           FrequencyPrediction = f.Prediction,
                           ModelAssistedPrediction = f.ModelAssistedFrequency,
                           AmountPrediction = a.Prediction,
                           ModelAssistedAmount = a.ModelAssistedAmount,
                           ShrinkageFactor = a.ShrinkageFactor,
                           NDays = a.NumberOfPositiveIntakeDays,
                           AmountsCofactor = a.Cofactor,
                           AmountsCovariable = a.Covariable,
                           FrequencyCofactor = f.Cofactor,
                           FrequencyCovariable = f.Covariable,
                           TransformedOIM = a.TransformedAmount,
                       };
                   })
               .ToList();
            return usualIntakeIndividuals;
        }
    }
}
