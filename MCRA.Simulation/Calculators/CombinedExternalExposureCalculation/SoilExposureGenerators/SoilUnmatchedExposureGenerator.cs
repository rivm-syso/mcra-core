using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {

    public class SoilUnmatchedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and reference individuals
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, soilIndividualDayExposures.Count);
            var selected = soilIndividualDayExposures.ElementAt(ix);
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, selected);
                results.Add(result);
            }
            return results;
        }
    }
}
