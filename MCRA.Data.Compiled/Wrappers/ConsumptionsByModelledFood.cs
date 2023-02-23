using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    public sealed class ConsumptionsByModelledFood {

        /// <summary>
        /// The consuming individual.
        /// </summary>
        public Individual Individual { 
            get {
                return IndividualDay.Individual;
            }
        }

        /// <summary>
        /// The day of consumption.
        /// </summary>
        public string Day {
            get {
                return IndividualDay.IdDay;
            }
        }

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
        public Food FoodAsEaten {
            get {
                return FoodConsumption?.Food;
            }
        }

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
        public double AmountProcessedFoodAsMeasured {
            get {
                return AmountFoodAsMeasured / ProportionProcessing;
            }
        }

        public string ProcessingFacetCode() {
            return (ProcessingTypes?.Any() ?? false)
                ? string.Join("-", ProcessingTypes.Select(r => r.Code))
                : null;
        }
    }
}
