using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the consumption of foods as eaten from input data
    /// </summary>
    public sealed class ConsumptionDataSection : SummarySection {

        [Display(AutoGenerateField = false)]
        public List<ConsumptionFrequencyRecord> ConsumptionFrequencies { get; set; }

        [DisplayName("Individual-days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividualDays { get; set; }

        [DisplayName("Total consumptions modelled food")]
        public int TotalConsumptionsFoodAsEaten { get; set; }

        [DisplayName("Total consumptions modelled food")]
        public int TotalConsumptionsFoodAsMeasured { get; set; }

        [DisplayName("Total number of food as eaten")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double NumberOfFoodAsEaten { get; set; }

        [DisplayName("Total number of modelled food")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double NumberOfFoodAsMeasured { get; set; }

        /// <summary>
        /// Summarize the consumption days of the individuals in the survey.
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="consumptions"></param>
        /// <param name="consumptionsByModelledFood"></param>
        public void Summarize(
            ICollection<IndividualDay> individualDays,
            ICollection<FoodConsumption> consumptions,
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood
        ) {
            TotalIndividualDays = individualDays.Count();

            TotalConsumptionsFoodAsEaten = consumptions.Count();
            NumberOfFoodAsEaten = consumptions.Select(r => r.Food).Distinct().Count();

            if (consumptionsByModelledFood != null) {
                var consumerDays = consumptionsByModelledFood.Select(r => r.IndividualDay).Distinct().ToList();
                NumberOfFoodAsMeasured = consumptionsByModelledFood.Select(gr => gr.FoodAsMeasured).Distinct().Count();
                TotalConsumptionsFoodAsMeasured = consumptionsByModelledFood.Count();
                ConsumptionFrequencies = consumerDays.Any()
                    ? computeConsumptionFrequencies(consumerDays, individualDays)
                    : new List<ConsumptionFrequencyRecord>();
            } else {
                var consumerDays = consumptions.Select(r => r.IndividualDay).Distinct().ToList();
                TotalConsumptionsFoodAsMeasured = 0;
                NumberOfFoodAsMeasured = double.NaN;
                ConsumptionFrequencies = consumerDays.Any()
                    ? computeConsumptionFrequencies(consumerDays, individualDays)
                    : new List<ConsumptionFrequencyRecord>();
            }
        }

        private static List<ConsumptionFrequencyRecord> computeConsumptionFrequencies(
            ICollection<IndividualDay> consumptionDays,
            ICollection<IndividualDay> allIndividualDays
        ) {
            var groupedConsumerDays = consumptionDays.ToLookup(r => r.Individual);
            var intakeFrequencies = allIndividualDays
                .GroupBy(r => r.Individual)
                .Select(g => (
                    AllDays: g.Count(),
                    ConsumptionsDays: groupedConsumerDays.Contains(g.Key) ? groupedConsumerDays[g.Key].Count() : 0
                ))
                .ToList();
            var consumptionFrequencies = new List<ConsumptionFrequencyRecord>();
            var nDay = intakeFrequencies.Select(c => c.AllDays).Max();
            for (int i = 0; i < nDay + 1; i++) {
                var count = intakeFrequencies.Count(c => c.ConsumptionsDays == i);
                consumptionFrequencies.Add(new ConsumptionFrequencyRecord() {
                    NumberOfDays = i,
                    NumberOfIndividuals = count,
                    PercentageOfAllIndividuals = (double)count / intakeFrequencies.Count * 100
                });
            }

            return consumptionFrequencies;
        }
    }
}
