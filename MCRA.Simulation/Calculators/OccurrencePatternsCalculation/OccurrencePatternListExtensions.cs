using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public static class OccurrencePatternListExtensions {

        /// <summary>
        /// Gets all agricultural uses that apply for the specified location.
        /// </summary>
        /// <param name="agriculturalUses"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ICollection<OccurrencePattern> FilterByLocation(
            this ICollection<OccurrencePattern> agriculturalUses,
            string location
        ) {
            var generalAgriculturalUses = agriculturalUses
                .Where(r => string.IsNullOrEmpty(r.Location))
                .ToList();
            var locationAgriculturalUses = agriculturalUses
                .Where(auol => (location ?? "").Equals(auol.Location, StringComparison.OrdinalIgnoreCase))
                .ToList();
            locationAgriculturalUses.AddRange(
                generalAgriculturalUses.Where(
                    gau => !locationAgriculturalUses
                            .Any(sau => sau.Code.Equals(gau.Code, StringComparison.OrdinalIgnoreCase))
                )
            );
            return locationAgriculturalUses;
        }

        /// <summary>
        /// Gets all agricultural uses that are generally defined (i.e., not location specific).
        /// </summary>
        /// <param name="agriculturalUses"></param>
        /// <returns></returns>
        public static ICollection<OccurrencePattern> FilterByCompound(
            this ICollection<OccurrencePattern> agriculturalUses,
            Compound compound
        ) {
            var result = agriculturalUses
                .Where(r => r.Compounds.Contains(compound))
                .ToList();
            return result;
        }
    }
}
