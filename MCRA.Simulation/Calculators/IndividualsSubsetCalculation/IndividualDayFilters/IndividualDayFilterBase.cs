using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualDayFilters {
    public abstract class IndividualDayFilterBase : IPropertyIndividualDayFilter {

        /// <summary>
        /// The individual day property associated with this filter.
        /// </summary>
        public IndividualProperty IndividualProperty { get; set; }

        /// <summary>
        /// Specifies whether individual days with unspecified properties
        /// should be included or not.
        /// </summary>
        public bool IncludeMissingValueRecords { get; set; } = true;

        public IndividualDayFilterBase(
            IndividualProperty property,
            bool includeMissingRecords
        ) {
            IndividualProperty = property;
            IncludeMissingValueRecords = includeMissingRecords;
        }

        /// <summary>
        /// Returns whether the given individual day passes the filter.
        /// </summary>
        /// <param name="individualDay"></param>
        /// <returns></returns>
        public abstract bool Passes(IndividualDay individualDay);
    }
}
