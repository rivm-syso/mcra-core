using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations {

    public sealed class ChronicScreeningCalculator : ScreeningCalculator {

        public ChronicScreeningCalculator(
            double criticalExposurePercentage,
            double cumulativeSelectionPercentage,
            double importanceLor,
            bool isPerPerson
        ) : base(criticalExposurePercentage, cumulativeSelectionPercentage, importanceLor, isPerPerson) {
        }

        /// <summary>
        /// Performs intake screening of all relevant source-compound combinations.
        /// </summary>
        /// <param name="conversionResults"></param>
        /// <param name="individualDays"></param>
        /// <param name="foodConsumptions"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="correctedRelativePotencyFactors"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public override ScreeningResult Calculate(
            IEnumerable<FoodConversionResult> conversionResults,
            IEnumerable<IndividualDay> individualDays,
            IEnumerable<FoodConsumption> foodConsumptions,
            IEnumerable<CompoundResidueCollection> compoundResidueCollections,
            IDictionary<Compound, double> correctedRelativePotencyFactors,
            CompositeProgressState progressState
        ) {
            var groupedConsumptions = foodConsumptions.ToLookup(fc => fc.Food);

            var groupedFoodConversions = conversionResults
                .Where(fcr => groupedConsumptions.Contains(fcr.FoodAsEaten))
                .GroupBy(c => (
                    compound: c.Compound,
                    foodAsEaten: c.FoodAsEaten,
                    foodAsMeasured: c.FoodAsMeasured
                ));

            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();

            if (groupedFoodConversions.All(c => c.Key.compound == null)) {
                _screeningResultRecords = correctedRelativePotencyFactors
                    .Keys
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .SelectMany(c => {
                        return groupedFoodConversions
                            .Select(k => new ScreeningResultRecord {
                                Compound = c,
                                FoodAsEaten = k.Key.foodAsEaten,
                                FoodAsMeasured = k.Key.foodAsMeasured,
                                AggregateProportion = k.Sum(p => p.Proportion),
                            })
                            .ToList();
                    }).ToList();
            } else {
                _screeningResultRecords = groupedFoodConversions
                    .Select(k => new ScreeningResultRecord {
                        Compound = k.Key.compound,
                        FoodAsEaten = k.Key.foodAsEaten,
                        FoodAsMeasured = k.Key.foodAsMeasured,
                        AggregateProportion = k.Sum(p => p.Proportion),
                    })
                    .ToList();
            }

            var individualDayCounts = individualDays
                .GroupBy(id => id.Individual)
                .ToDictionary(id => id.Key, id => id.Count());


            Parallel.ForEach(_screeningResultRecords, new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken }, record => {
                var rpf = correctedRelativePotencyFactors[record.Compound];
                var compoundResidueCollection = compoundResidueCollections.Single(c => c.Compound == record.Compound && c.Food == record.FoodAsMeasured);
                calculateExposureDistributions(record, groupedConsumptions[record.FoodAsEaten], individualDayCounts, compoundResidueCollection, rpf);
            });

            calculateCriticalExposureContributions(progressState);

            var result = new ScreeningResult() {
                TotalNumberOfSccRecords = _screeningResultRecords.Count,
                RiskDriver = _screeningResultRecords.FirstOrDefault(),
                ScreeningResultsPerFoodCompound = getSelectedScreeningResultsPerFoodCompound(),
                SumCupAllSccRecords = _screeningResultRecords.Sum(r => r.Cup),
            };
            return result;
        }

        /// <summary>
        /// Calculate parameters for consumption, concentration, detect and censored distributions
        /// </summary>
        /// <param name="screeningResult"></param>
        /// <param name="consumptions"></param>
        /// <param name="totalIndividualDays"></param>
        /// <param name="compoundResidueCollection"></param>
        /// <param name="rpf"></param>
        private void calculateExposureDistributions(
            ScreeningResultRecord screeningResult,
            IEnumerable<FoodConsumption> consumptions,
            Dictionary<Individual, int> individualDayCounts,
            CompoundResidueCollection compoundResidueCollection,
            double rpf
        ) {
            var individualsCount = individualDayCounts.Count;
            var result = consumptions
                .Where(co => individualDayCounts.ContainsKey(co.Individual))
                .GroupBy(co => co.Individual)
                .Select(c => (
                    logAmounts: Math.Log(c.Sum(a => a.Amount) / individualDayCounts[c.Key] / (_isPerPerson ? 1 : c.Key.BodyWeight)),
                    samplingWeights: c.Key.SamplingWeight
                ))
                .ToList();

            var logAmounts = result.Select(c => c.logAmounts).ToList();
            var weights = result.Select(c => c.samplingWeights).ToList();
            var logAmountsCount = logAmounts.Count();
            var variance = logAmounts.Variance(weights);

            var consumptionParam = new ConsumptionDistributionParameters() {
                Mu = logAmounts.Average(weights) + Math.Log(screeningResult.AggregateProportion),
                Sigma = double.IsNaN(Math.Sqrt(variance)) ? 0 : Math.Sqrt(variance),
                Fraction = (double)logAmountsCount / individualsCount,
            };

            var logRPF = Math.Log(rpf);
            var logPositives = compoundResidueCollection.Positives.Select(c => Math.Log(c)).ToList();
            var logLors = compoundResidueCollection.CensoredValues.Where(c => c > 0D).Select(c => Math.Log(c)).ToList();
            var totalResidues = logPositives.Count + logLors.Count;

            var concentrationParam = new ConcentrationDistributionParameters() {
                FractionPositives = logPositives.Count > 0 ? (double)logPositives.Count / totalResidues : 0,
                FractionCensoredValues = logLors.Count > 0 ? (double)logLors.Count / totalResidues : 0,
                MuPositives = (logPositives.Count > 0) ? logPositives.Average() + logRPF : Math.Log(_epsilon),
                SigmaPositives = !double.IsNaN(logPositives.Variance()) ? Math.Sqrt(logPositives.Variance()) : _epsilon,
                MuCensoredValues = (logLors.Count > 0) ? logLors.Average() + logRPF : Math.Log(_epsilon),
            };

            screeningResult.ConsumptionParameters = consumptionParam;
            screeningResult.ConcentrationParameters = concentrationParam;
        }

        /// <summary>
        /// Updates the contributions to the critical exposure level of all screening records.
        /// </summary>
        /// <param name="settings"></param>
        protected override void calculateCriticalExposureContributions(CompositeProgressState progressState = null) {
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();

            Parallel.ForEach(_screeningResultRecords, new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken }, result => {
                calculateCriticalExposureContribution(result);
            });


            var logExposureMaximum = Math.Log(_screeningResultRecords.Max(c => c.Exposure));
            Parallel.ForEach(
                _screeningResultRecords,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                item => {
                    var upperPercentile = item.DetectParameters.Fraction
                                        * (1 - NormalDistribution.CDF(
                                                  item.DetectParameters.Mu,
                                                  item.DetectParameters.Sigma,
                                                  (logExposureMaximum - item.DetectParameters.Mu) / item.DetectParameters.Sigma
                                              )
                                          );
                    item.Cup = upperPercentile;
                }
            );
            var sumcup = _screeningResultRecords.Select(c => c.Cup).Sum();
            _screeningResultRecords = _screeningResultRecords.OrderByDescending(r => r.Cup).ToList();
            _screeningResultRecords.ForEach(c => c.CupPercentage = c.Cup / sumcup);
        }

        /// <summary>
        /// Calculate critical exposure for supplied critical exposure percentage
        /// </summary>
        /// <param name="screeningResult"></param>
        protected override void calculateCriticalExposureContribution(ScreeningResultRecord screeningResult) {

            var consumptionParam = screeningResult.ConsumptionParameters;
            var concentrationParam = screeningResult.ConcentrationParameters;

            screeningResult.CensoredValueParameters = new ScreeningDistributionParameters() {
                Fraction = 0,
                Mu = double.NaN,
                Sigma = double.NaN,
            };

            var muCensoredValues = concentrationParam.MuCensoredValues + _logLorImportanceFactor;

            var meanConcentration = concentrationParam.FractionPositives * Math.Exp(concentrationParam.MuPositives)
                                  + concentrationParam.FractionCensoredValues * Math.Exp(muCensoredValues);

            screeningResult.DetectParameters = new ScreeningDistributionParameters() {
                Fraction = consumptionParam.Fraction * (concentrationParam.FractionPositives + concentrationParam.FractionCensoredValues),
                Mu = Math.Log(meanConcentration) + consumptionParam.Mu,
                Sigma = (consumptionParam.Sigma > 0) ? Math.Sqrt(Math.Pow(consumptionParam.Sigma, 2)) : _epsilon,
            };

            var criticalExposureFraction = _criticalExposurePercentage / 100;
            var correctedCriticalExposureFraction = (criticalExposureFraction - (1 - consumptionParam.Fraction)) / consumptionParam.Fraction;
            if (consumptionParam.Fraction == 0 || correctedCriticalExposureFraction <= 0) {
                screeningResult.Exposure = 0;
                screeningResult.WeightCensoredValue = double.NaN;
                screeningResult.WeightDetect = double.NaN;
            } else {
                screeningResult.WeightCensoredValue = double.NaN;
                screeningResult.WeightDetect = double.NaN;

                var zpCorrected = NormalDistribution.InvCDF(0, 1, correctedCriticalExposureFraction) > 0 ? NormalDistribution.InvCDF(0, 1, correctedCriticalExposureFraction) : 0;
                var logExposure = screeningResult.DetectParameters.Mu + screeningResult.DetectParameters.Sigma * zpCorrected;
                screeningResult.Exposure = Math.Exp(logExposure);
            }
        }
    }
}
