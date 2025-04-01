using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {
    public interface IFocalCommodityMeasurementReplacementCalculator {

        Dictionary<Food, SampleCompoundCollection> Compute(
            IDictionary<Food, SampleCompoundCollection> baseSampleCompoundCollections,
            ICollection<(Food Food, Compound Substance)> focalCombinations,
            IRandom generator
        );
    }
}