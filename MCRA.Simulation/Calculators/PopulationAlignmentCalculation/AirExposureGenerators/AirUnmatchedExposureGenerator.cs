using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.AirExposureGenerators {

    public class AirUnmatchedExposureGenerator : AirExposureGenerator {

        /// <summary>
        /// Randomly pair dust and reference individuals
        /// </summary>
        protected override List<AirIndividualDayExposure> createAirIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, airIndividualDayExposures.Count);
            var selected = airIndividualDayExposures.ElementAt(ix);
            var results = new List<AirIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = selected.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}
