using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class ConsumptionsByModelledFoodCalculator {

        public ConsumptionsByModelledFoodCalculator() {
        }

        public ICollection<ConsumptionsByModelledFood> Compute(
            ICollection<FoodConversionResult> conversionResults,
            ICollection<FoodConsumption> selectedFoodConsumptions,
            CompositeProgressState progressReport
        ) {
            var cancelToken = progressReport?.CancellationToken ?? new CancellationToken();
            var conversionLookup = conversionResults.ToLookup(c => c.FoodAsEaten);

            var conversionsByFae = conversionResults
                .AsParallel()
                .WithCancellation(cancelToken)
                .GroupBy(r => r.FoodAsEaten);

            var conversionsByFaeAndConversionPath = conversionsByFae
                .AsParallel()
                .WithCancellation(cancelToken)
                .ToDictionary(g => g.Key, g => {
                    return g.GroupBy(r => (r.FoodAsMeasured, r.AllStepsToMeasuredString))
                        .Select(r => r.ToDictionary(fcr => fcr.Compound ?? SimulationConstants.NullSubstance));
                });

            var result = selectedFoodConsumptions
                .AsParallel()
                .WithDegreeOfParallelism(200)
                .WithCancellation(cancelToken)
                .SelectMany(r => conversionsByFaeAndConversionPath[r.Food], (fc, fcr) => {
                    var amountFoodAsMeasured = fc.Amount * fcr.Values.Max(r => r.Proportion);
                    return new ConsumptionsByModelledFood() {
                        IndividualDay = fc.IndividualDay,
                        FoodConsumption = fc,
                        ConversionResultsPerCompound = fcr,
                        AmountFoodAsMeasured = amountFoodAsMeasured,
                        ProportionProcessing = fcr.Values.Max(r => r.ProportionProcessing),
                        IsBrand = fcr.Values.First().MarketShare < 1,
                        ProcessingTypes = fcr.Values.FirstOrDefault(r => r.ProcessingTypes?.Any() ?? false)?.ProcessingTypes?.ToList(),
                        FoodAsMeasured = fcr.Values.FirstOrDefault()?.FoodAsMeasured,
                    };
                })
                .ToList();
            return result;
        }

        public static IDictionary<(Individual, string), List<ConsumptionsByModelledFood>> CreateIndividualDayLookUp(
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood
        ) {
            // Populate the individual days consumption cache
            var individualDayConsumptionCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            foreach (var c in consumptionsByModelledFood) {
                if (!individualDayConsumptionCache.TryGetValue((c.Individual, c.Day), out var consumptions)) {
                    individualDayConsumptionCache.Add((c.Individual, c.Day), new List<ConsumptionsByModelledFood> { c });
                } else {
                    consumptions.Add(c);
                }
            }
            return individualDayConsumptionCache;
        }
    }
}
