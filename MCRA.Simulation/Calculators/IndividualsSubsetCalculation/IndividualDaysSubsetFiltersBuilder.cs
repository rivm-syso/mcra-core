using MCRA.Utils.DateTimes;
using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualDayFilters;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public sealed class IndividualDaysSubsetFiltersBuilder {

        /// <summary>
        /// Creates a collection of individual day filters based on the
        /// population and the subset settings that can be used to filter
        /// or subset individual days.
        /// </summary>
        public List<IPropertyIndividualDayFilter> Create(
            Population population,
            IndividualSubsetType individualSubsetType,
            ICollection<string> selectedFilterPropertyKeys,
            bool includeMissingValueRecords
        ) {
            return Create(
                population?.PopulationIndividualPropertyValues?.Values,
                individualSubsetType,
                selectedFilterPropertyKeys,
                includeMissingValueRecords
            );
        }

        /// <summary>
        /// Creates a collection of individual day filters based on the
        /// provided population individual properties and the subset settings
        /// that can be used to filter or subset individual days.
        /// </summary>
        public List<IPropertyIndividualDayFilter> Create(
            ICollection<PopulationIndividualPropertyValue> populationIndividualProperties,
            IndividualSubsetType individualSubsetType,
            ICollection<string> selectedFilterPropertyKeys,
            bool includeMissingValueRecords
        ) {
            if (individualSubsetType == IndividualSubsetType.IgnorePopulationDefinition
                || populationIndividualProperties == null
            ) {
                return null;
            }
            var populationIndividualDayProperties = populationIndividualProperties
                .Where(c => c.IndividualProperty.PropertyLevel == PropertyLevelType.IndividualDay)
                .ToList();
            if (individualSubsetType == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                populationIndividualDayProperties = populationIndividualDayProperties
                    .Where(r => selectedFilterPropertyKeys.Contains(r.IndividualProperty.Code))
                    .ToList();
            }
            var result = populationIndividualDayProperties
                .Select(r => createIndividualDayFilter(r, includeMissingValueRecords))
                .ToList();
            return result;
        }

        private IPropertyIndividualDayFilter createIndividualDayFilter(
            PopulationIndividualPropertyValue populationPropertyValue,
            bool includeMissingValueRecords
        ) {
            switch (populationPropertyValue.IndividualProperty.PropertyType) {
                case IndividualPropertyType.Month:
                    return new IndividualDayMonthsFilter(
                        populationPropertyValue.IndividualProperty,
                        populationPropertyValue.GetMonths(),
                        includeMissingValueRecords
                    );
                case IndividualPropertyType.DateTime:
                    return new IndividualDayPeriodFilter(
                        populationPropertyValue.IndividualProperty,
                        new TimeRange() {
                            StartDate = populationPropertyValue.StartDate.Value,
                            EndDate = populationPropertyValue.EndDate.Value
                        },
                        includeMissingValueRecords
                    );
                default:
                    // TODO
                    throw new NotImplementedException($"Individual days filter not available for individual day properties of type {populationPropertyValue.IndividualProperty.PropertyType}.");
            }

        }
    }
}
