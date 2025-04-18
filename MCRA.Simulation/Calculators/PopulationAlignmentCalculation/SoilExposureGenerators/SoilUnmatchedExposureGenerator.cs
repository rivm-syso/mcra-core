using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.SoilExposureGenerators {

    public class SoilUnmatchedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and reference individuals
        /// </summary>
        protected override List<SoilIndividualDayExposure> createSoilIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, soilIndividualDayExposures.Count);
            var selected = soilIndividualDayExposures.ElementAt(ix);
            var results = new List<SoilIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = selected.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}
