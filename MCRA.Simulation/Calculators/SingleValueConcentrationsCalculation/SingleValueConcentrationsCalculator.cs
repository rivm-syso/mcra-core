using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;

namespace MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation {
    public sealed class SingleValueConcentrationsCalculator {

        public IDictionary<(Food, Compound), SingleValueConcentrationModel> Compute(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            ICollection<SampleCompoundCollection> substanceSampleCollections,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits
        ) {
            var percentages = new double[] { 50, 97.5 };
            var result = new Dictionary<(Food, Compound), SingleValueConcentrationModel>();
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder();
            var compoundResidueCollections = compoundResidueCollectionsBuilder
                .Create(substances, substanceSampleCollections, null, null);
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    var record = new SingleValueConcentrationModel() {
                        Food = food,
                        Substance = substance,
                    };
                    var hasValue = false;
                    if (compoundResidueCollections != null && compoundResidueCollections.TryGetValue((food, substance), out var substanceResidueCollection) && substanceResidueCollection.NumberOfResidues > 0) {
                        var positives = substanceResidueCollection.Positives.Any() ? substanceResidueCollection.Positives : null;
                        var lors = substanceResidueCollection.CensoredValues.Any() ? substanceResidueCollection.CensoredValues : null;
                        record.MeanConcentration = positives?.Average() ?? double.NaN;
                        record.Loq = lors?.Max() ?? double.NaN;
                        record.HighestConcentration = positives?.Max() ?? double.NaN;
                        record.Percentiles = positives?
                            .Percentiles(percentages)
                            .Select((r, ix) => (percentages[ix], r))
                            .ToList();
                        hasValue = true;
                    }
                    if (maximumConcentrationLimits != null && maximumConcentrationLimits.TryGetValue((food, substance), out var maximumResidueLimit)) {
                        record.Mrl = maximumResidueLimit.Limit;
                        hasValue = true;
                    }
                    if (hasValue) {
                        result.Add((food, substance), record);
                    }
                }
            }
            return result;
        }
    }
}
