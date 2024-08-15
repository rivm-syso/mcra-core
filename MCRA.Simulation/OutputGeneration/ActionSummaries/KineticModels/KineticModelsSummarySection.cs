using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticModelsSummarySection : SummarySection {

        public List<AbsorptionFactorRecord> AbsorptionFactorRecords { get; set; }

        /// <summary>
        /// Summarize absorption factors.
        /// </summary>
        public void SummarizeAbsorptionFactors(
            ICollection<KineticAbsorptionFactor> kineticAbsorptionFactors,
            ICollection<Compound> substances,
            bool aggregate
        ) {
            AbsorptionFactorRecords = new List<AbsorptionFactorRecord>();
            var defaults = new List<AbsorptionFactorRecord>();

            var potentialSubstanceRouteCombination = new Dictionary<(ExposurePathType Route, Compound Substance), bool>();
            foreach (var substance in substances) {
                potentialSubstanceRouteCombination[(ExposurePathType.Oral, substance)] = false;
                if (aggregate) {
                    potentialSubstanceRouteCombination[(ExposurePathType.Dermal, substance)] = false;
                    potentialSubstanceRouteCombination[(ExposurePathType.Inhalation, substance)] = false;
                }
            }

            foreach (var item in kineticAbsorptionFactors) {

                var isSpecified = kineticAbsorptionFactors?
                    .Any(c =>  c.Substance != null) ?? false;
                var record = new AbsorptionFactorRecord() {
                    CompoundCode = item.Substance.Code,
                    CompoundName = item.Substance.Name,
                    Route = item.ExposureRoute.ToString(),
                    AbsorptionFactor = item.AbsorptionFactor,
                    IsDefault = "default",
                };
                if (isSpecified && potentialSubstanceRouteCombination.TryGetValue((item.ExposureRoute, item.Substance), out var present)) {
                    potentialSubstanceRouteCombination[(item.ExposureRoute, item.Substance)] = true;
                    AbsorptionFactorRecords.Add(record);
                } else {
                    defaults.Add(record);
                }
            }
            var routes = potentialSubstanceRouteCombination
                .Where(c => !c.Value)
                .Select(c => c.Key.Route)
                .Distinct()
                .ToList();
            foreach (var route in routes) {
                var record = defaults.Single(c => c.Route.ToString() == route.ToString());
                AbsorptionFactorRecords.Add(record);
            }
        }
    }
}
