using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Objects {

    public sealed class ConsumptionsByModelledFood {

        /// <summary>
        /// The consuming individual.
        /// </summary>
        public Individual Individual => IndividualDay.Individual;

        /// <summary>
        /// The day of consumption.
        /// </summary>
        public string Day => IndividualDay.IdDay;

        /// <summary>
        /// The individual day of the modelled food consumption.
        /// </summary>
        public IndividualDay IndividualDay { get; set; }

        /// <summary>
        /// The consumption containing the consumed food and consumed amount.
        /// </summary>
        public FoodConsumption FoodConsumption { get; set; }

        /// <summary>
        /// The conversion steps from the consumed food to the measured food (per substance).
        /// </summary>
        public Dictionary<Compound, FoodConversionResult> ConversionResultsPerCompound { get; set; }

        /// <summary>
        /// The consumed amount of the modelled food.
        /// </summary>
        public double AmountFoodAsMeasured { get; set; }

        /// <summary>
        /// Is food a brand or not. A food is a brand only when there are marketshares.
        /// </summary>
        public bool IsBrand { get; set; }

        /// <summary>
        /// The consumed food.
        /// </summary>
        public Food FoodAsEaten => FoodConsumption?.Food;

        /// <summary>
        /// The measured food.
        /// </summary>
        public Food FoodAsMeasured { get; set; }

        /// <summary>
        /// The processing types associated with the consumption in case the
        /// food is a processed food / raw-commodity derivative.
        /// </summary>
        public List<ProcessingType> ProcessingTypes { get; set; }

        /// <summary>
        /// The amount of the processed modelled food.
        /// </summary>
        public double ProportionProcessing { get; set; }

        /// <summary>
        /// The amount of the processed modelled food.
        /// </summary>
        public double AmountProcessedFoodAsMeasured => AmountFoodAsMeasured / ProportionProcessing;

        public string ProcessingFacetCode() {
            return (ProcessingTypes?.Count > 0)
                ? string.Join("-", ProcessingTypes.Select(r => r.Code))
                : null;
        }
    }
}
