using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations {

    public sealed class AcuteScreeningCalculator : ScreeningCalculator {

        public AcuteScreeningCalculator(
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
           
            var totalIndividualDays = individualDays.Count();
            Parallel.ForEach(_screeningResultRecords, new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                record => {
                    var rpf = correctedRelativePotencyFactors[record.Compound];
                    var compoundResidueCollection = compoundResidueCollections.Single(c => c.Compound == record.Compound && c.Food == record.FoodAsMeasured);
                    calculateExposureDistributions(record, groupedConsumptions[record.FoodAsEaten], totalIndividualDays, compoundResidueCollection, rpf);
                }
            );

            calculateCriticalExposureContributions(progressState);

            var result = new ScreeningResult() {
                TotalNumberOfSccRecords = _screeningResultRecords.Count,
                RiskDriver = _screeningResultRecords.FirstOrDefault(),
                ScreeningResultsPerFoodCompound = getSelectedScreeningResultsPerFoodCompound(),
                SumCupAllSccRecords = _screeningResultRecords.Sum(r => r.Cup)
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
            int totalIndividualDays,
            CompoundResidueCollection compoundResidueCollection,
            double rpf
        ) {
            var result = consumptions
                .GroupBy(co => (co.idDay, co.Individual))
                .Select(c => (
                    logAmounts: Math.Log(c.Sum(a => a.Amount) / (_isPerPerson ? 1 : c.Key.Individual.BodyWeight)),
                    samplingWeights: c.Key.Individual.SamplingWeight
                ))
                .ToList();

            var logAmounts = result.Select(c => c.logAmounts).ToList();
            var weights = result.Select(c => c.samplingWeights).ToList();
            var logAmountsCount = logAmounts.Count;
            var variance = logAmounts.Variance(weights);

            screeningResult.ConsumptionParameters = new ConsumptionDistributionParameters() {
                Mu = logAmounts.Average(weights) + Math.Log(screeningResult.AggregateProportion),
                Sigma = double.IsNaN(Math.Sqrt(variance)) ? 0 : Math.Sqrt(variance),
                Fraction = (double)logAmountsCount / totalIndividualDays,
            };

            var logRPF = Math.Log(rpf);
            var logPositives = compoundResidueCollection.Positives.Select(c => Math.Log(c)).ToList();
            var logLors = compoundResidueCollection.CensoredValues.Where(c => c > 0D).Select(c => Math.Log(c)).ToList();
            var totalResidues = logPositives.Count + logLors.Count;

            screeningResult.ConcentrationParameters = new ConcentrationDistributionParameters() {
                FractionPositives = logPositives.Count > 0 ? (double)logPositives.Count / totalResidues : 0,
                FractionCensoredValues = logLors.Count > 0 ? (double)logLors.Count / totalResidues : 0,
                MuPositives = (logPositives.Count > 0) ? logPositives.Average() + logRPF : Math.Log(_epsilon),
                SigmaPositives = !double.IsNaN(logPositives.Variance()) ? Math.Sqrt(logPositives.Variance()) : _epsilon,
                MuCensoredValues = logLors.Count > 0 ? logLors.Average() + logRPF : Math.Log(_epsilon),
            };
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
            Parallel.ForEach(_screeningResultRecords, new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken }, item => {
                var upperPercentile = item.ConcentrationParameters.FractionCensoredValues
                                        * (1 - NormalDistribution.CDF(0, 1, (logExposureMaximum - item.CensoredValueParameters.Mu) / item.CensoredValueParameters.Sigma))
                                    + item.ConcentrationParameters.FractionPositives
                                        * (1 - NormalDistribution.CDF(0, 1, (logExposureMaximum - item.DetectParameters.Mu) / item.DetectParameters.Sigma));

                item.Cup = _importanceLor == 0
                         ? (item.ConcentrationParameters.FractionPositives > 0
                             ? upperPercentile * item.ConsumptionParameters.Fraction
                             : 0)
                         : upperPercentile * item.ConsumptionParameters.Fraction;
            });
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
                Fraction = consumptionParam.Fraction * concentrationParam.FractionCensoredValues, // TODO: include agricultural use percentage
                Mu = _importanceLor > 0 ? consumptionParam.Mu + concentrationParam.MuCensoredValues + _logLorImportanceFactor : Math.Log(_epsilon),
                Sigma = (_importanceLor > 0 && consumptionParam.Sigma > 0) ? consumptionParam.Sigma : _epsilon,
            };

            screeningResult.DetectParameters = new ScreeningDistributionParameters() {
                Fraction = consumptionParam.Fraction * concentrationParam.FractionPositives,
                Mu = consumptionParam.Mu + concentrationParam.MuPositives,
                Sigma = (consumptionParam.Sigma + concentrationParam.SigmaPositives > 0)
                      ? Math.Sqrt(Math.Pow(consumptionParam.Sigma, 2) + Math.Pow(concentrationParam.SigmaPositives, 2))
                      : _epsilon,
            };

            var criticalExposureFraction = _criticalExposurePercentage / 100;
            var correctedCriticalExposureFraction = (criticalExposureFraction - (1 - consumptionParam.Fraction)) / consumptionParam.Fraction;
            if (consumptionParam.Fraction == 0 || correctedCriticalExposureFraction <= 0) {
                screeningResult.Exposure = 0;
                screeningResult.WeightCensoredValue = double.NaN;
                screeningResult.WeightDetect = double.NaN;
            } else {
                //bisection search
                var zpCorrected = NormalDistribution.InvCDF(0, 1, correctedCriticalExposureFraction) > 0 ? NormalDistribution.InvCDF(0, 1, correctedCriticalExposureFraction) : 0;
                var censoredValue = screeningResult.CensoredValueParameters;
                var detect = screeningResult.DetectParameters;
                var lower = Math.Min(censoredValue.Mu, detect.Mu) - 4 * Math.Max(censoredValue.Sigma, detect.Sigma);
                var upper = Math.Max(censoredValue.Mu, detect.Mu) + zpCorrected * Math.Max(censoredValue.Sigma, detect.Sigma);
                var logExposure = StatisticalTests.BiSectionSearch(
                    limit => getFunction(limit, concentrationParam.FractionPositives, censoredValue, detect),
                    lower,
                    upper,
                    100,
                    correctedCriticalExposureFraction,
                    0.000001
                );
                var fCensoredValue = censoredValue.Fraction * NormalDistribution.PDF(censoredValue.Mu, censoredValue.Sigma, logExposure);
                var fDetect = detect.Fraction * NormalDistribution.PDF(detect.Mu, detect.Sigma, logExposure);
                screeningResult.WeightCensoredValue = fCensoredValue / (fCensoredValue + fDetect);
                screeningResult.WeightDetect = 1 - screeningResult.WeightCensoredValue;
                screeningResult.Exposure = Math.Exp(logExposure);
            }
        }

        /// <summary>
        /// Function for bisection algorithm
        /// </summary>
        /// <param name="z"></param>
        /// <param name="fraction"></param>
        /// <param name="censoredValue"></param>
        /// <param name="detect"></param>
        /// <returns></returns>
        private double getFunction(double z, double fraction, ScreeningDistributionParameters censoredValue, ScreeningDistributionParameters detect) {
            return (1 - fraction) * NormalDistribution.CDF(0, 1, (z - censoredValue.Mu) / censoredValue.Sigma) + fraction * NormalDistribution.CDF(0, 1, (z - detect.Mu) / detect.Sigma);
        }
    }
}
