using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {
    public interface IFocalCommodityMeasurementReplacementCalculator {

        Dictionary<Food, SampleCompoundCollection> Compute(
            IDictionary<Food, SampleCompoundCollection> baseSampleCompoundCollections,
            ICollection<(Food Food, Compound Substance)> focalCombinations,
            IRandom generator
        );
    }
}