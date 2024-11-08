using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class CovariateGroupCalculator {

        private readonly CovariateModelType _covariateModelTypeFrequencyModel;
        private readonly CovariateModelType _covariateModelTypeAmountsModel;
        private readonly List<double> _predictionLevels;

        public CovariateGroupCalculator(
            List<double> predictionLevels,
            CovariateModelType covariateModelTypeFrequencyModel,
            CovariateModelType covariateModelTypeAmountsModel
        ) {
            _covariateModelTypeFrequencyModel = covariateModelTypeFrequencyModel;
            _covariateModelTypeAmountsModel = covariateModelTypeAmountsModel;
            _predictionLevels = predictionLevels;
        }

        /// <summary>
        /// Computes the data-based covariate groups.
        /// </summary>
        /// <param name="individualDayAmounts"></param>
        /// <returns></returns>
        public List<CovariateGroup> ComputeDataBasedCovariateGroups(
            ICollection<SimpleIndividualDayIntake> individualDayAmounts
        ) {
            var intakeFrequencies = individualDayAmounts
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(r => (
                    SamplingWeight: r.First().IndividualSamplingWeight,
                    Covariable: r.First().Individual.Covariable,
                    Cofactor: r.First().Individual.Cofactor
                ))
                .ToList();

            var result = new List<CovariateGroup>();

            if (_covariateModelTypeFrequencyModel == CovariateModelType.Constant
                && _covariateModelTypeAmountsModel == CovariateModelType.Constant
            ) {
                //Constant 
                return [
                    new CovariateGroup() {
                        Covariable = double.NaN,
                        Cofactor = null,
                        NumberOfIndividuals = intakeFrequencies.Count,
                        GroupSamplingWeight = intakeFrequencies.Sum(r => r.SamplingWeight),
                    },
                ];
            } else if (
                (_covariateModelTypeFrequencyModel == CovariateModelType.Cofactor && _covariateModelTypeAmountsModel == CovariateModelType.Constant)
                || (_covariateModelTypeFrequencyModel == CovariateModelType.Cofactor && _covariateModelTypeAmountsModel == CovariateModelType.Cofactor)
                || (_covariateModelTypeFrequencyModel == CovariateModelType.Constant && _covariateModelTypeAmountsModel == CovariateModelType.Cofactor)
            ) {
                //Cofactor
                var freqCofact = intakeFrequencies
                    .GroupBy(fr => fr.Cofactor)
                    .Select(g => (
                        count: g.Count(),
                        cofactor: g.Key,
                        covariable: double.NaN,
                        sumSamplingWeight: g.Sum(c => c.SamplingWeight)
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var cofactor = freqCofact.Select(c => c.cofactor).ToArray();
                var covariable = freqCofact.Select(c => c.covariable).ToArray();
                var count = freqCofact.Select(c => c.count).ToArray();
                var sumSamplingWeights = freqCofact.Select(c => c.sumSamplingWeight).ToArray();

                for (int i = 0; i < cofactor.Length; i++) {
                    result.Add(new CovariateGroup() {
                        Cofactor = cofactor[i],
                        Covariable = covariable[i],
                        NumberOfIndividuals = count[i],
                        GroupSamplingWeight = sumSamplingWeights[i]
                    });
                }
                return result;
            } else if (
                (_covariateModelTypeFrequencyModel == CovariateModelType.Covariable && _covariateModelTypeAmountsModel == CovariateModelType.Constant)
                || (_covariateModelTypeFrequencyModel == CovariateModelType.Covariable && _covariateModelTypeAmountsModel == CovariateModelType.Covariable)
                || (_covariateModelTypeFrequencyModel == CovariateModelType.Constant && _covariateModelTypeAmountsModel == CovariateModelType.Covariable)
            ) {
                //Covariable
                var freqCovar = intakeFrequencies
                      .GroupBy(fr => fr.Covariable)
                      .Select(g => (
                          count: g.Count(),
                          cofactor: (string)null,
                          covariable: g.Key,
                          sumSamplingWeight: g.Sum(c => c.SamplingWeight)
                      ))
                      .OrderBy(a => a.covariable)
                      .ToList();
                var cofactor = freqCovar.Select(c => c.cofactor).ToArray();
                var covariable = freqCovar.Select(c => c.covariable).ToArray();
                var count = freqCovar.Select(c => c.count).ToArray();
                var sumSamplingWeights = freqCovar.Select(c => c.sumSamplingWeight).ToArray();
                for (int i = 0; i < cofactor.Length; i++) {
                    result.Add(new CovariateGroup() {
                        Cofactor = cofactor[i],
                        Covariable = covariable[i],
                        NumberOfIndividuals = count[i],
                        GroupSamplingWeight = sumSamplingWeights[i]
                    });
                }
                return result;
            } else {
                //CovariableCofactor
                var freqCovarCofact = intakeFrequencies
                    .GroupBy(fr => (fr.Cofactor, fr.Covariable))
                    .Select(g => (
                        count: g.Count(),
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        sumSamplingWeight: g.Sum(c => c.SamplingWeight)
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();
                var cofactor = freqCovarCofact.Select(c => c.cofactor).ToArray();
                var covariable = freqCovarCofact.Select(c => c.covariable).ToArray();
                var count = freqCovarCofact.Select(c => c.count).ToArray();
                var sumSamplingWeights = freqCovarCofact.Select(c => c.sumSamplingWeight).ToArray();
                for (int i = 0; i < cofactor.Length; i++) {
                    result.Add(new CovariateGroup() {
                        Cofactor = cofactor[i],
                        Covariable = covariable[i],
                        NumberOfIndividuals = count[i],
                        GroupSamplingWeight = sumSamplingWeights[i]
                    });
                }
                return result;
            }
        }

        /// <summary>
        /// Compute specified covariate groups.
        /// </summary>
        /// <param name="individualDayAmounts"></param>
        /// <returns></returns>
        public List<CovariateGroup> ComputeSpecifiedPredictionsCovariateGroups(
            ICollection<SimpleIndividualDayIntake> individualDayAmounts
        ) {
            var result = new List<CovariateGroup>();

            if (_covariateModelTypeFrequencyModel == CovariateModelType.Constant && _covariateModelTypeAmountsModel == CovariateModelType.Constant) {
                //Constant 
                return [
                    new CovariateGroup() {
                        Covariable = double.NaN,
                        Cofactor = null,
                    },
                ];
            } else if ((_covariateModelTypeFrequencyModel == CovariateModelType.Cofactor && _covariateModelTypeAmountsModel == CovariateModelType.Constant) ||
                  (_covariateModelTypeFrequencyModel == CovariateModelType.Cofactor && _covariateModelTypeAmountsModel == CovariateModelType.Cofactor) ||
                  (_covariateModelTypeFrequencyModel == CovariateModelType.Constant && _covariateModelTypeAmountsModel == CovariateModelType.Cofactor)
                  ) {
                //Cofactor
                var factorLevels = individualDayAmounts
                    .Select(c => c.Individual.Cofactor)
                    .Distinct()
                    .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                for (int i = 0; i < factorLevels.Count; i++) {
                    result.Add(new CovariateGroup() {
                        Cofactor = factorLevels[i],
                        Covariable = double.NaN,
                    });
                }
                return result;
            } else if ((_covariateModelTypeFrequencyModel == CovariateModelType.Covariable && _covariateModelTypeAmountsModel == CovariateModelType.Constant) ||
                  (_covariateModelTypeFrequencyModel == CovariateModelType.Covariable && _covariateModelTypeAmountsModel == CovariateModelType.Covariable) ||
                  (_covariateModelTypeFrequencyModel == CovariateModelType.Constant && _covariateModelTypeAmountsModel == CovariateModelType.Covariable)
                  ) {
                //Covariable
                for (int i = 0; i < _predictionLevels.Count; i++) {
                    result.Add(new CovariateGroup() {
                        Cofactor = null,
                        Covariable = _predictionLevels[i],
                    });
                }
                return result;
            } else {
                //CovariableCofactor
                var factorLevels = individualDayAmounts
                    .Select(c => c.Individual.Cofactor)
                    .Distinct()
                    .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                for (int i = 0; i < factorLevels.Count; i++) {
                    for (int j = 0; j < _predictionLevels.Count; j++) {
                        result.Add(new CovariateGroup() {
                            Cofactor = factorLevels[i],
                            Covariable = _predictionLevels[j],
                        });
                    }
                }
                return result;
            }
        }
    }
}