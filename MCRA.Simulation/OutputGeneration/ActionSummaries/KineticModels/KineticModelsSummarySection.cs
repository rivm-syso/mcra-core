using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticModelsSummarySection : SummarySection {

        public List<AbsorptionFactorRecord> AbsorptionFactorRecords { get; set; } = [];

        /// <summary>
        /// Summarize absorption factors.
        /// </summary>
        public void SummarizeAbsorptionFactors(
            ICollection<SimpleAbsorptionFactor> absorptionFactors,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes
        ) {
            // TODO: the code below should be refactored
            var defaults = new List<AbsorptionFactorRecord>();
            var potentialSubstanceRouteCombination = new Dictionary<(ExposureRoute Route, Compound Substance), bool>();
            foreach (var substance in substances) {
                if (routes.Contains(ExposureRoute.Oral)) {
                    potentialSubstanceRouteCombination[(ExposureRoute.Oral, substance)] = false;
                }
                if (routes.Contains(ExposureRoute.Dermal)) {
                    potentialSubstanceRouteCombination[(ExposureRoute.Dermal, substance)] = false;
                }
                if (routes.Contains(ExposureRoute.Inhalation)) {
                    potentialSubstanceRouteCombination[(ExposureRoute.Inhalation, substance)] = false;
                }
            }

            foreach (var item in absorptionFactors) {
                var isSpecified = absorptionFactors?
                    .Any(c =>  c.Substance != null) ?? false;
                var record = new AbsorptionFactorRecord() {
                    CompoundCode = item.Substance.Code,
                    CompoundName = item.Substance.Name,
                    Route = item.ExposureRoute.ToString(),
                    AbsorptionFactor = item.AbsorptionFactor,
                    IsDefault = "default",
                };
                if (isSpecified && potentialSubstanceRouteCombination
                    .TryGetValue((item.ExposureRoute, item.Substance), out var present)) {
                    potentialSubstanceRouteCombination[(item.ExposureRoute, item.Substance)] = true;
                    AbsorptionFactorRecords.Add(record);
                } else {
                    defaults.Add(record);
                }
            }
            var routeCombinations = potentialSubstanceRouteCombination
                .Where(c => !c.Value)
                .Select(c => c.Key.Route)
                .Distinct()
                .ToList();
            foreach (var route in routeCombinations) {
                var record = defaults.Single(c => c.Route.ToString() == route.ToString());
                AbsorptionFactorRecords.Add(record);
            }
        }
    }
}
