using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DustExposureGenerators {

    public class DustUnmatchedExposureGenerator : DustExposureGenerator {

        /// <summary>
        /// Randomly pair dust and reference individuals
        /// </summary>
        protected override List<DustIndividualDayExposure> createDustIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, dustIndividualDayExposures.Count);
            var selected = dustIndividualDayExposures.ElementAt(ix);
            var results = new List<DustIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = selected.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}
