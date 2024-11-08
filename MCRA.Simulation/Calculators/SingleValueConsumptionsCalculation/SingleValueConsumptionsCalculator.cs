using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class SingleValueConsumptionsCalculator {
        private readonly ISingleValueConsumptionsCalculatorSettings _settings;

        public SingleValueConsumptionsCalculator(ISingleValueConsumptionsCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Computes single value consumption models from consumptions by food-as-measured.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="individuals"></param>
        /// <param name="consumptionsByModelledFood"></param>
        /// <param name="consumptionUnit"></param>
        /// <param name="bodyWeightUnit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="targetBodyWeightUnit"></param>
        /// <returns></returns>
        public ICollection<SingleValueConsumptionModel> Compute(
                ICollection<IndividualDay> simulatedIndividualDays,
                ICollection<Individual> individuals,
                ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood,
                ConsumptionUnit consumptionUnit,
                BodyWeightUnit bodyWeightUnit,
                ConsumptionIntakeUnit targetUnit,
                BodyWeightUnit targetBodyWeightUnit
            ) {
            if (targetBodyWeightUnit != BodyWeightUnit.kg) {
                throw new NotImplementedException("target body weight unit should be kg");
            }
            if (!targetUnit.IsPerPerson() && targetBodyWeightUnit != targetUnit.GetBodyWeightUnit()) {
                throw new NotImplementedException("target body weight unit should be equal to single value consumption body weight unit.");
            }
            var percentages = new double[] { 50, 97.5 };
            var consumptionsByFoodAsMeasured = _settings.IsProcessing
                ? consumptionsByModelledFood.GroupBy(c => (c.FoodAsMeasured, c.ProcessingFacetCode()))
                : consumptionsByModelledFood.GroupBy(c => (c.FoodAsMeasured, (string)null));

            var consumptionAmountCorrectionFactor = consumptionUnit.GetMultiplicationFactor(targetUnit.GetConsumptionUnit());
            var bodyWeightCorrectionFactor = bodyWeightUnit.GetBodyWeightUnitMultiplier(targetBodyWeightUnit);

            var result = consumptionsByFoodAsMeasured
                .AsParallel()
                .Select(foodAsMeasuredConsumptions => {
                    var food = foodAsMeasuredConsumptions.Key.FoodAsMeasured;
                    if (_settings.IsProcessing && (foodAsMeasuredConsumptions.First().ProcessingTypes?.Count > 0)) {
                        return computeSingleValue(
                            food,
                            foodAsMeasuredConsumptions.First().ProcessingTypes,
                            simulatedIndividualDays,
                            individuals,
                            foodAsMeasuredConsumptions,
                            foodAsMeasuredConsumptions.First().ProportionProcessing,
                            percentages,
                            targetUnit.IsPerPerson(),
                            _settings,
                            consumptionAmountCorrectionFactor,
                            bodyWeightCorrectionFactor
                        );
                    } else {
                        return computeSingleValue(
                            food,
                            null,
                            simulatedIndividualDays,
                            individuals,
                            foodAsMeasuredConsumptions,
                            double.NaN,
                            percentages,
                            targetUnit.IsPerPerson(),
                            _settings,
                            consumptionAmountCorrectionFactor,
                            bodyWeightCorrectionFactor
                         );
                    }
                })
                .ToList();
            return result;
        }

        private  SingleValueConsumptionModel computeSingleValue(
            Food food,
            ICollection<ProcessingType> processingTypes,
            ICollection<IndividualDay> individualDays,
            ICollection<Individual> allIndividuals,
            IEnumerable<ConsumptionsByModelledFood> consumptionsFoodAsMeasuredRecords,
            double processingCorrectionFactor,
            double[] percentages,
            bool expressSingleValueConsumptionsPerPerson,
            ISingleValueConsumptionsCalculatorSettings settings,
            double consumptionAmountCorrectionFactor,
            double bodyWeightCorrectionFactor
        ) {

            Func<ConsumptionsByModelledFood, double> getConsumption;
            if (processingTypes != null) {
                getConsumption = x => x.AmountProcessedFoodAsMeasured;
            } else {
                getConsumption = x => x.AmountFoodAsMeasured;
            }
            if (settings.ExposureType == ExposureType.Acute) {
                var totalConsumptionAmount = expressSingleValueConsumptionsPerPerson && !_settings.UseBodyWeightStandardisedConsumptionDistribution
                    ? consumptionsFoodAsMeasuredRecords.Sum(co => getConsumption(co) * co.Individual.SamplingWeight)
                    : consumptionsFoodAsMeasuredRecords.Sum(co => getConsumption(co) * co.Individual.SamplingWeight / co.Individual.BodyWeight);

                var groupedPositiveConsumptionDays = consumptionsFoodAsMeasuredRecords
                    .Where(c => getConsumption(c) > 0)
                    .GroupBy(co => (co.Day, co.Individual));

                var amountsPositives = expressSingleValueConsumptionsPerPerson && !_settings.UseBodyWeightStandardisedConsumptionDistribution
                    ? groupedPositiveConsumptionDays.Select(r => r.Sum(c => getConsumption(c))).ToList()
                    : groupedPositiveConsumptionDays.Select(r => r.Sum(c => getConsumption(c)) / r.First().Individual.BodyWeight).ToList();

                var samplingWeightsPositives = settings.UseSamplingWeights
                    ? groupedPositiveConsumptionDays.Select(r => r.First().Individual.SamplingWeight).ToList()
                    : groupedPositiveConsumptionDays.Select(r => 1D).ToList();

                if (settings.IsConsumersOnly) {
                    var bodyWeight = bodyWeightCorrectionFactor * groupedPositiveConsumptionDays.Average(c => c.Key.Individual.BodyWeight);

                    var percentiles = amountsPositives?
                            .PercentilesWithSamplingWeights(samplingWeightsPositives, percentages)
                            .Select((r, ix) => (Percentage: percentages[ix], Factor: consumptionAmountCorrectionFactor * r))
                            .ToList();
                    if (expressSingleValueConsumptionsPerPerson && _settings.UseBodyWeightStandardisedConsumptionDistribution) {
                        percentiles = percentiles.Select(c => (c.Percentage, c.Factor * bodyWeight)).ToList();
                        totalConsumptionAmount *= bodyWeight;
                    }

                    var result = new SingleValueConsumptionModel() {
                        Food = food.BaseFood ?? food,
                        ProcessingTypes = processingTypes,
                        ProcessingCorrectionFactor = processingCorrectionFactor,
                        MeanConsumption = consumptionAmountCorrectionFactor * totalConsumptionAmount / samplingWeightsPositives.Sum(),
                        Percentiles = percentiles,
                        BodyWeight = bodyWeight,
                    };
                    result.LargePortion = result.GetPercentile(97.5);
                    return result;
                } else {
                    var sumSamplingWeightAllDays = settings.UseSamplingWeights
                        ? individualDays.Sum(r => r.Individual.SamplingWeight)
                        : individualDays.Count;
                    var bodyWeight = bodyWeightCorrectionFactor * individualDays.Average(c => c.Individual.BodyWeight);
                    var percentiles = amountsPositives?
                            .PercentilesAdditionalZeros(samplingWeightsPositives, percentages, sumSamplingWeightAllDays - samplingWeightsPositives.Sum())
                            .Select((r, ix) => (Percentage: percentages[ix], Factor: consumptionAmountCorrectionFactor * r))
                            .ToList();
                    if (expressSingleValueConsumptionsPerPerson && _settings.UseBodyWeightStandardisedConsumptionDistribution) {
                        percentiles = percentiles.Select(c => (c.Percentage, c.Factor * bodyWeight)).ToList();
                        totalConsumptionAmount *= bodyWeight;
                    }
                    var result = new SingleValueConsumptionModel() {
                        Food = food.BaseFood ?? food,
                        ProcessingTypes = processingTypes,
                        ProcessingCorrectionFactor = processingCorrectionFactor,
                        MeanConsumption = consumptionAmountCorrectionFactor * totalConsumptionAmount / sumSamplingWeightAllDays,
                        Percentiles = percentiles,
                        BodyWeight = bodyWeight,
                    };
                    result.LargePortion = result.GetPercentile(97.5);
                    return result;
                }
            } else {
                var individualDaysLookup = individualDays.ToLookup(r => r.Individual);

                var totalConsumptionAmount = expressSingleValueConsumptionsPerPerson && !_settings.UseBodyWeightStandardisedConsumptionDistribution
                    ? consumptionsFoodAsMeasuredRecords.Sum(co => co.AmountFoodAsMeasured * co.Individual.SamplingWeight / individualDaysLookup[co.Individual].Count())
                    : consumptionsFoodAsMeasuredRecords.Sum(co => co.AmountFoodAsMeasured * co.Individual.SamplingWeight / individualDaysLookup[co.Individual].Count() / co.Individual.BodyWeight);

                var groupedConsumptionIndividuals = consumptionsFoodAsMeasuredRecords
                   .GroupBy(co => (co.Individual));

                var amountsPositives = expressSingleValueConsumptionsPerPerson && !_settings.UseBodyWeightStandardisedConsumptionDistribution
                    ? groupedConsumptionIndividuals.Select(r => r.Sum(c => c.AmountFoodAsMeasured) / individualDaysLookup[r.First().Individual].Count()).Where(c => c > 0).ToList()
                    : groupedConsumptionIndividuals.Select(r => r.Sum(c => c.AmountFoodAsMeasured) / individualDaysLookup[r.First().Individual].Count() / r.First().Individual.BodyWeight).Where(c => c > 0).ToList();

                var samplingWeightsPositives = groupedConsumptionIndividuals
                    .Select(r => settings.UseSamplingWeights ? r.First().Individual.SamplingWeight : 1D)
                    .ToList();

                if (settings.IsConsumersOnly) {
                    var bodyWeight = bodyWeightCorrectionFactor * groupedConsumptionIndividuals.Average(c => c.Key.BodyWeight);
                    var percentiles = amountsPositives?
                            .PercentilesWithSamplingWeights(samplingWeightsPositives, percentages)
                            .Select((r, ix) => (Percentage: percentages[ix], Factor: consumptionAmountCorrectionFactor * r))
                            .ToList();
                    if (expressSingleValueConsumptionsPerPerson && _settings.UseBodyWeightStandardisedConsumptionDistribution) {
                        percentiles = percentiles.Select(c => (c.Percentage, c.Factor * bodyWeight)).ToList();
                        totalConsumptionAmount *= bodyWeight;
                    }

                    var result = new SingleValueConsumptionModel() {
                        Food = food.BaseFood ?? food,
                        ProcessingTypes = processingTypes,
                        ProcessingCorrectionFactor = processingCorrectionFactor,
                        MeanConsumption = consumptionAmountCorrectionFactor * totalConsumptionAmount / samplingWeightsPositives.Sum(),
                        Percentiles = percentiles,
                        BodyWeight = bodyWeight,
                    };
                    result.LargePortion = result.GetPercentile(97.5);
                    return result;

                } else {
                    var sumSamplingWeightAllDays = settings.UseSamplingWeights
                        ? allIndividuals.Sum(r => r.SamplingWeight)
                        : allIndividuals.Count;
                    var bodyWeight = bodyWeightCorrectionFactor * allIndividuals.Average(c => c.BodyWeight);
                    var percentiles = amountsPositives?
                            .PercentilesAdditionalZeros(samplingWeightsPositives, percentages, sumSamplingWeightAllDays - samplingWeightsPositives.Sum())
                            .Select((r, ix) => (Percentage: percentages[ix], Factor: consumptionAmountCorrectionFactor * r))
                            .ToList();
                    if (expressSingleValueConsumptionsPerPerson && _settings.UseBodyWeightStandardisedConsumptionDistribution) {
                        percentiles = percentiles.Select(c => (c.Percentage, c.Factor * bodyWeight)).ToList();
                        totalConsumptionAmount *= bodyWeight;
                    }
                    var result = new SingleValueConsumptionModel() {
                        Food = food.BaseFood ?? food,
                        ProcessingTypes = processingTypes,
                        ProcessingCorrectionFactor = processingCorrectionFactor,
                        MeanConsumption = consumptionAmountCorrectionFactor * totalConsumptionAmount / sumSamplingWeightAllDays,
                        Percentiles = percentiles,
                        BodyWeight = bodyWeight,
                    };
                    result.LargePortion = result.GetPercentile(97.5);
                    return result;
                }
            }
        }
    }
}
